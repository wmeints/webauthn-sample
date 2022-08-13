namespace WebAuthnSample.Services;

public class AssertPublicKeyCredentialResult
{
    public List<string> ErrorMessages { get; } = new();
    public bool Succeeded => ErrorMessages.Count == 0;
}