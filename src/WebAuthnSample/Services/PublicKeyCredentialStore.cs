using Microsoft.EntityFrameworkCore;
using WebAuthnSample.Data;
using WebAuthnSample.Models;

namespace WebAuthnSample.Services;

/// <summary>
/// Manages storage of public key credentials in the application.
/// </summary>
public class PublicKeyCredentialStore
{
    private readonly ApplicationDbContext _applicationDbContext;

    /// <summary>
    /// Initializes a new instance of <see cref="PublicKeyCredentialStore"/>
    /// </summary>
    /// <param name="applicationDbContext">Application DbContext to use for storage.</param>
    public PublicKeyCredentialStore(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    /// <summary>
    /// Gets all public key credentials stored for an application user.
    /// </summary>
    /// <param name="userName">The UserName of the user.</param>
    /// <returns>Returns a list of public key credentials if any exist.</returns>
    public async Task<IEnumerable<PublicKeyCredential>> GetCredentialsByUserNameAsync(string userName)
    {
        return await _applicationDbContext.PublicKeyCredentials
            .Where(x => x.User.UserName == userName)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if no other public key credentials exist for the specified credential ID.
    /// </summary>
    /// <param name="credentialId">Credential identifier to search for.</param>
    /// <returns>Returns <c>true</c> when no credential exists with the specified ID; Otherwise <c>false</c>.</returns>
    public async Task<bool> IsCredentialUnique(byte[] credentialId)
    {
        var credentialIdValue = Convert.ToBase64String(credentialId);
        return !await _applicationDbContext.PublicKeyCredentials.AnyAsync(x => x.CredentialId == credentialIdValue);
    }

    /// <summary>
    /// Creates a new public key credential for a user.
    /// </summary>
    /// <param name="storedCredential">Credential information to store.</param>
    public async Task CreateAsync(PublicKeyCredential storedCredential)
    {
        await _applicationDbContext.PublicKeyCredentials.AddAsync(storedCredential);
        await _applicationDbContext.SaveChangesAsync();
    }
}