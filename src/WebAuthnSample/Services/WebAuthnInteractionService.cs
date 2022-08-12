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

    /// <summary>
    /// Initializes a new instance of <see cref="WebAuthnInteractionService"/>
    /// </summary>
    /// <param name="fido2">FIDO2 library instance.</param>
    /// <param name="publicKeyCredentialStore">The public key credential store to use.</param>
    /// <param name="userManager">The user manager to use for managing the application user account.</param>
    public WebAuthnInteractionService(Fido2 fido2, PublicKeyCredentialStore publicKeyCredentialStore,
        UserManager<ApplicationUser> userManager)
    {
        _fido2 = fido2;
        _publicKeyCredentialStore = publicKeyCredentialStore;
        _userManager = userManager;
    }

    /// <summary>
    /// Generates options to create a new public key credential
    /// </summary>
    /// <param name="displayName">Display name to use for the user.</param>
    /// <param name="userName">User name to use for the user.</param>
    /// <returns>The options to use for creating a new public key credential.</returns>
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
                UserHandle = registrationResult.User.Id,
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
}