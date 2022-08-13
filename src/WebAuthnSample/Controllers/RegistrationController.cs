using Fido2NetLib;
using Microsoft.AspNetCore.Mvc;
using WebAuthnSample.Forms;
using WebAuthnSample.Services;

namespace WebAuthnSample.Controllers;

/// <summary>
/// This controller is responsible for the endpoints that are used in the registration process.
/// </summary>
[ApiController]
public class RegistrationController : ControllerBase
{
    private readonly IWebAuthnInteractionService _webAuthnInteractionService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RegistrationController(IWebAuthnInteractionService webAuthnInteractionService, IHttpContextAccessor httpContextAccessor)
    {
        _webAuthnInteractionService = webAuthnInteractionService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost("/api/registration/options")]
    public async Task<IActionResult> GenerateCredentialOptions([FromBody]GenerateCredentialOptionsForm form)
    {
        var result = await _webAuthnInteractionService.GenerateCredentialsCreateOptionsAsync(
            form.DisplayName, form.UserName);

        if (result.Succeeded)
        {
            _httpContextAccessor?.HttpContext?.Session.SetString("fido2.options", result.Options.ToJson());
            return Content(result.Options.ToJson(), "application/json");
        }
        else
        {
            foreach (var error in result.ErrorMessages)
            {
                ModelState.AddModelError("", error);
            }
            
            return BadRequest(ModelState);
        }
    }

    [HttpPost("/api/registration/complete")]
    public async Task<IActionResult> CompleteCredentialRegistration(
        [FromBody] AuthenticatorAttestationRawResponse attestationResponse)
    {
        // Retrieve the original options that were used to start the registration.
        // We'll need these to verify that the user is completing an existing registration request.
        var credentialCreateOptions = CredentialCreateOptions.FromJson(
            _httpContextAccessor?.HttpContext?.Session.GetString("fido2.options"));
        
        var result = await _webAuthnInteractionService.CreatePublicKeyCredentialAsync(
            credentialCreateOptions, attestationResponse);

        if (result.Succeeded)
        {
            return Accepted();
        }
        else
        {
            foreach (var error in result.ErrorMessages)
            {
                ModelState.AddModelError("", error);
            }

            return BadRequest(ModelState);
        }
    }
}