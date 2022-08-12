using Microsoft.AspNetCore.Identity;
using NSec.Cryptography;

namespace WebAuthnSample.Models;

/// <summary>
/// Represents a user that can register and log-in on the website.
/// </summary>
public class ApplicationUser: IdentityUser
{
    /// <summary>
    /// The registered public key credentials for the user.
    /// </summary>
    public List<PublicKeyCredential> PublicKeyCredentials { get; set; } = new();
}