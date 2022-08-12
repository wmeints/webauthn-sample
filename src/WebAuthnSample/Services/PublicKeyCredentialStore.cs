using Microsoft.EntityFrameworkCore;
using WebAuthnSample.Data;
using WebAuthnSample.Models;

namespace WebAuthnSample.Services;

public class PublicKeyCredentialStore
{
    private readonly ApplicationDbContext _applicationDbContext;

    public PublicKeyCredentialStore(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<IEnumerable<PublicKeyCredential>> GetCredentialsByUserNameAsync(string formUserName)
    {
        return await _applicationDbContext.PublicKeyCredentials
            .Where(x => x.User.UserName == formUserName)
            .ToListAsync();
    }

    public async Task<bool> IsCredentialUnique(byte[] credentialId)
    {
        var credentialIdValue = Convert.ToBase64String(credentialId);
        return await _applicationDbContext.PublicKeyCredentials.AnyAsync(x => x.CredentialId == credentialIdValue);
    }
}