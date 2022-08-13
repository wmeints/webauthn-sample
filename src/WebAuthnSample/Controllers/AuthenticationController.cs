using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using WebAuthnSample.Forms;
using WebAuthnSample.Services;

namespace WebAuthnSample.Controllers;

[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebAuthnInteractionService _webAuthnInteractionService;

    public AuthenticationController(IHttpContextAccessor httpContextAccessor,
        IWebAuthnInteractionService webAuthnInteractionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _webAuthnInteractionService = webAuthnInteractionService;
    }

    [HttpPost("/api/authentication/options")]
    public async Task<IActionResult> GenerateAssertionOptions([FromBody]GenerateAssertionOptionsForm form)
    {
        var result = await _webAuthnInteractionService.GenerateAssertionOptionsAsync(
            form.UserName,
            UserVerificationRequirement.Required);

        if (!result.Succeeded)
        {
            foreach (var error in result.ErrorMessages)
            {
                ModelState.AddModelError("", error);
            }

            return BadRequest(ModelState);
        }

        _httpContextAccessor.HttpContext?.Session.SetString("fido2.assertionOptions", result.Options.ToJson());
        return Content(result.Options.ToJson(), "application/json");
    }

    [HttpPost("/api/authentication/login")]
    public async Task<IActionResult> MakeAssertion([FromBody] AuthenticatorAssertionRawResponse assertionRawResponse)
    {
        var assertionOptions = AssertionOptions.FromJson(_httpContextAccessor.HttpContext?.Session.GetString(
            "fido2.assertionOptions"));
        
        var result = await _webAuthnInteractionService.AssertPublicKeyCredentialAsync(
            assertionOptions, assertionRawResponse);

        if (result.Succeeded)
        {
            return Ok();
        }

        foreach (var error in result.ErrorMessages)
        {
            ModelState.AddModelError("", error);    
        }
            
        return BadRequest(ModelState);
    }
}