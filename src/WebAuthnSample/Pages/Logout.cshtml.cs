using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAuthnSample.Models;

namespace WebAuthnSample.Pages;

public class Logout : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public Logout(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGetAsync(string returnUrl)
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect(returnUrl);
    }
}