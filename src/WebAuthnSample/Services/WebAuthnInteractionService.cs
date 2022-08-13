using System.Text;
using System.Transactions;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Identity;
using WebAuthnSample.Models;

namespace WebAuthnSample.Services;

/// <summary>
/// Implements interaction logic for the WebAuthn flow in the application.
/// </summary>
public class WebAuthnInteractionService : IWebAuthnInteractionService
{
    private readonly Fido2 _fido2;
    private readonly PublicKeyCredentialStore _publicKeyCredentialStore;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>
    /// Initializes a new instance of <see cref="WebAuthnInteractionService"/>
    /// </summary>
    /// <param name="fido2">FIDO2 library instance.</param>
    /// <param name="publicKeyCredentialStore">The public key credential store to use.</param>
    /// <param name="userManager">The user manager to use for managing the application user account.</param>
    /// <param name="signInManager">The sign-in manager to use for logging in the user.</param>
    public WebAuthnInteractionService(Fido2 fido2, PublicKeyCredentialStore publicKeyCredentialStore,
        UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _fido2 = fido2;
        _publicKeyCredentialStore = publicKeyCredentialStore;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    /// Generates options to create a new public key credential
    /// </summary>
    /// <param name="displayName">Display name to use for the user.</param>
    /// <param name="userName">User name to use for the user.</param>
    /// <returns>An awaitable task with the outcome of the operation.</returns>
    public async Task<GenerateCredentialsCreateOptionsResult> GenerateCredentialsCreateOptionsAsync(string displayName,
        string userName)
    {
        var result = new GenerateCredentialsCreateOptionsResult();

        if (string.IsNullOrEmpty(displayName))
        {
            result.ErrorMessages.Add("Display name is not provided.");
        }

        if (string.IsNullOrEmpty(userName))
        {
            result.ErrorMessages.Add("UserName is not provided.");
        }

        if (await _userManager.FindByNameAsync(userName) is { })
        {
            result.ErrorMessages.Add("User is already registered.");
        }

        if (!result.Succeeded)
        {
            return result;
        }

        // This is not an application user, but rather a set of properties
        // to identify a potential user later on in the registration process.
        var user = new Fido2User
        {
            Name = userName,
            DisplayName = displayName,
            Id = Encoding.UTF8.GetBytes(userName)
        };

        // We want to use the USB key as the authenticator (hence: CrossPlatform for authenticator attachment)
        // We don't want the user to enter a PIN or password before registering the new credential.
        var authenticatorSelection = new AuthenticatorSelection
        {
            AuthenticatorAttachment = AuthenticatorAttachment.CrossPlatform,
            RequireResidentKey = false,
            UserVerification = UserVerificationRequirement.Discouraged
        };

        // We create the options for registering a new public key credential for the user with the following options:
        // - We don't want a verifyable attestion: This allows users to remain relatively anonymous.
        // - We want to use the authenticator options we created earlier (USB Key + No user verification).
        // - We want to exclude any keys the user already registered.
        var options = _fido2.RequestNewCredential(
            user,
            new List<PublicKeyCredentialDescriptor>(),
            authenticatorSelection,
            AttestationConveyancePreference.None
        );

        result.Options = options;

        return result;
    }

    /// <summary>
    /// Creates a new user with a public key credential attached to it.
    /// </summary>
    /// <param name="credentialCreateOptions">The options used for the registration process.</param>
    /// <param name="attestationResponse">The attestion response provided by the user in the browser.</param>
    /// <returns>An awaitable task with the outcome of the operation.</returns>
    public async Task<CreatePublicKeyCredentialResult> CreatePublicKeyCredentialAsync(
        CredentialCreateOptions credentialCreateOptions, AuthenticatorAttestationRawResponse attestationResponse)
    {
        var result = new CreatePublicKeyCredentialResult();

        // This check verifies that the request isn't a reply from an earlier attempt.
        IsCredentialIdUniqueToUserAsyncDelegate isCredentialUniqueCallback =
            async (args, cancellationToken) =>
                await _publicKeyCredentialStore.IsCredentialUnique(args.CredentialId);

        // Make the new public key credential based on the options and the data provided
        // by the user in the frontend. This information contains the so-called attestation response.
        // Basically, this is a response to the challenge and the public key information of the user.
        var success = await _fido2.MakeNewCredentialAsync(
            attestationResponse,
            credentialCreateOptions,
            isCredentialUniqueCallback);

        // If we've successfully completed the verification step, we can create an application user
        // and a public key credential for the user.
        if (success.Result is { } registrationResult)
        {
            var applicationUser = new ApplicationUser
            {
                Email = credentialCreateOptions.User.Name,
                UserName = credentialCreateOptions.User.Name,
                EmailConfirmed = true,
            };

            var storedCredential = new PublicKeyCredential
            {
                User = applicationUser,
                Descriptor = new PublicKeyCredentialDescriptor(registrationResult.CredentialId),
                PublicKey = registrationResult.PublicKey,
                UserHandle = Convert.ToBase64String(registrationResult.User.Id),
                SignatureCounter = registrationResult.Counter,
                CredentialType = registrationResult.CredType,
                RegistrationDate = DateTime.UtcNow,
                AttestationGuid = registrationResult.Aaguid,
                CredentialId = Convert.ToBase64String(registrationResult.CredentialId)
            };

            // This transaction scope is important as we want the user and its credential
            // to be persisted together. If one of these operations fails, then the other must fail too.
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var createUserResult = await _userManager.CreateAsync(applicationUser);

                if (result.Succeeded)
                {
                    await _publicKeyCredentialStore.CreateAsync(storedCredential);
                    scope.Complete();
                }
                else
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        result.ErrorMessages.Add(error.Description);
                    }
                }
            }
        }
        else
        {
            result.ErrorMessages.Add(success.ErrorMessage);
        }

        return result;
    }

    /// <summary>
    /// Generates options used to authenticate a user using an existing public key credential.
    /// </summary>
    /// <param name="userName">The user name of the user.</param>
    /// <param name="userVerification">The user verification requirement setting.</param>
    /// <returns>An awaitable task with the outcome of the operation.</returns>
    public async Task<GenerateAssertionOptionsResult> GenerateAssertionOptionsAsync(string userName, UserVerificationRequirement userVerification)
    {
        var result = new GenerateAssertionOptionsResult();
        var identityUser = await _userManager.FindByNameAsync(userName);

        // We can't authenticate users that haven't been registered in the application.
        // We also don't want to allow locked user accounts to authenticate.
        if (identityUser == null)
        {
            result.ErrorMessages.Add("Invalid credentials. Please verify your input and try again.");
            return result;
        }

        // The user is allowed to login using any of the stored public key credentials.
        // This allows the user to register multiple FIDO2 keys in case one of them breaks.
        var registeredCredentials = await _publicKeyCredentialStore.GetCredentialsByUserNameAsync(userName);
        var allowedCredentials = registeredCredentials.Select(x => x.Descriptor).ToList();
        
        var extensions = new AuthenticationExtensionsClientInputs
        {
            UserVerificationMethod = true
        };

        var assertionOptions = _fido2.GetAssertionOptions(allowedCredentials, userVerification, extensions);

        result.Options = assertionOptions;

        return result;
    }

    /// <summary>
    /// Verifies an assertion response made against a previously generated set of assertion options. 
    /// </summary>
    /// <param name="assertionOptions">Assertion options to verify against.</param>
    /// <param name="assertionRawResponse">Assertion response received from the browser.</param>
    /// <returns>An awaitable task with the outcome of the operation.</returns>
    public async Task<AssertPublicKeyCredentialResult> AssertPublicKeyCredentialAsync(AssertionOptions assertionOptions,
        AuthenticatorAssertionRawResponse assertionRawResponse)
    {
        var result = new AssertPublicKeyCredentialResult();
        var credentials = await _publicKeyCredentialStore.GetCredentialsByIdAsync(assertionRawResponse.Id);

        if (credentials == null)
        {
            result.ErrorMessages.Add("Invalid credential.");
            return result;
        }

        async Task<bool> IsUserHandleOwnerOfCredentialIdAsync(
            IsUserHandleOwnerOfCredentialIdParams args, CancellationToken cancellationToken)
        {
            return await _publicKeyCredentialStore.IsCredentialOwnedByUserAsync(args.UserHandle, args.CredentialId);
        }

        try
        {
            var assertionResult = await _fido2.MakeAssertionAsync(
                assertionRawResponse, assertionOptions, credentials.PublicKey,
                credentials.SignatureCounter, IsUserHandleOwnerOfCredentialIdAsync);
            
            await _publicKeyCredentialStore.UpdateSignatureCounter(credentials.Id, assertionResult.Counter);
            await _signInManager.SignInAsync(credentials.User, isPersistent: false);
        }
        catch (Fido2VerificationException)
        {
            result.ErrorMessages.Add("Assertion failed. Can't authenticate the user with this assertion response.");
        }
        
        return result;
    }
}