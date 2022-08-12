namespace WebAuthnSample.Services;

/// <summary>
/// Contains information about the outcome of the
/// <see cref="WebAuthnInteractionService.CreatePublicKeyCredentialAsync"/> method.
/// </summary>
public class CreatePublicKeyCredentialResult
{
    /// <summary>
    /// Gets the error messages generated for the operation.
    /// </summary>
    public List<string> ErrorMessages { get; } = new();
    
    /// <summary>
    /// Gets whether the operation succeeded.
    /// </summary>
    public bool Succeeded => ErrorMessages.Count == 0;
}