using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAuthnSample.Forms;
using WebAuthnSample.Models;
using WebAuthnSample.Services;

namespace WebAuthnSample.Controllers;

/// <summary>
/// This controller is responsible for the endpoints that are used in the registration process.
/// </summary>
[ApiController]
public class RegistrationController: ControllerBase
{
    private readonly Fido2 _fido2;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PublicKeyCredentialStore _publicKeyCredentialStore;
    private readonly ISession _session;

    public RegistrationController(UserManager<ApplicationUser> userManager, PublicKeyCredentialStore publicKeyCredentialStore, Fido2 fido2, ISession session)
    {
        _userManager = userManager;
        _publicKeyCredentialStore = publicKeyCredentialStore;
        _fido2 = fido2;
        _session = session;
    }

    [HttpPost("/api/registration/credentialoptions")]
    public async Task<IActionResult> GenerateCredentialOptions(GenerateCredentialOptionsForm form)
    {
        // Make sure we're not registering the same user twice. Especially since we're allowing multiple public key
        // credentials for the same user account. Users can only add additional keys through their profile page
        // which should use a different technique to generate credentials.
        if (await _userManager.FindByNameAsync(form.UserName) is { })
        {
            ModelState.AddModelError("", "User with the same user name already exists.");    
        }
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new Fido2User
        {
            Name = form.UserName,
            DisplayName = form.DisplayName,
            Id = Encoding.UTF8.GetBytes(form.UserName)
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
        
        // Store the options in the current session.
        // We'll need these later to complete the registration of the user.
        _session.SetString("fido2.options", options.ToJson());

        return Content(options.ToJson(), "application/json");
    }

    [HttpPost("/api/registration/complete")]
    public async Task<IActionResult> CompleteCredentialRegistration([FromBody] AuthenticatorAttestationRawResponse attestationResponse)
    {
        try
        {
            var originalOptions = CredentialCreateOptions.FromJson(_session.GetString("fido2.options"));

            IsCredentialIdUniqueToUserAsyncDelegate isCredentialUniqueCallback =
                async (args, cancellationToken) =>
                    await _publicKeyCredentialStore.IsCredentialUnique(args.CredentialId);

            var success = await _fido2.MakeNewCredentialAsync(
                attestationResponse,
                originalOptions,
                isCredentialUniqueCallback);

            if (success.Result is { } registrationResult)
            {
                var storedCredential = new PublicKeyCredential
                {

                };

                //TODO: Store the credential in the database
            }
        }
        catch
        {
            //TODO: Implement proper error handling
        }

        return Accepted();
    }
}