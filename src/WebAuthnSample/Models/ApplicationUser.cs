using System.Text;
using Fido2NetLib;
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

    /// <summary>
    /// The display name for the user.
    /// </summary>
    public string FullName { get; set; } = "";

    public Fido2User ToFido2User()
    {
        return new Fido2User()
        {
            Id = Encoding.UTF8.GetBytes(UserName),
            Name = UserName,
            DisplayName = FullName
        };
    }
}