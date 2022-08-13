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

    /// <summary>
    /// Gets public key credentials based on the credential ID
    /// </summary>
    /// <param name="credentialId"></param>
    /// <returns></returns>
    public async Task<PublicKeyCredential?> GetCredentialsByIdAsync(byte[] credentialId)
    {
        var credentialIdValue = Convert.ToBase64String(credentialId);

        return await _applicationDbContext.PublicKeyCredentials
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.CredentialId == credentialIdValue);
    }

    /// <summary>
    /// Verifies if there's a public key credential with the given user handle and credential ID. 
    /// </summary>
    /// <param name="userHandle">User handle representing the binary ID of the user.</param>
    /// <param name="credentialId">ID of the credential.</param>
    /// <returns>Returns <c>true</c> when there's a match; Otherwise returns <c>false</c>.</returns>
    public async Task<bool> IsCredentialOwnedByUserAsync(byte[] userHandle, byte[] credentialId)
    {
        var userHandleValue = Convert.ToBase64String(userHandle);
        var credentialIdValue = Convert.ToBase64String(credentialId);

        return await _applicationDbContext.PublicKeyCredentials
            .AnyAsync(x => x.UserHandle == userHandleValue && x.CredentialId == credentialIdValue);
    }

    /// <summary>
    /// Updates the signature counter for a public key credential
    /// </summary>
    /// <param name="credentialsId">ID of the credential</param>
    /// <param name="assertionResultCounter">The new value for the signature counter.</param>
    public async Task UpdateSignatureCounter(int credentialsId, uint assertionResultCounter)
    {
        var credential = await _applicationDbContext.PublicKeyCredentials.SingleAsync(x => x.Id == credentialsId);
        credential.SignatureCounter = assertionResultCounter;

        await _applicationDbContext.SaveChangesAsync();
    }
}