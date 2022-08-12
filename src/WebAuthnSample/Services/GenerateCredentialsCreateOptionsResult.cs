using System.Diagnostics.CodeAnalysis;
using Fido2NetLib;

namespace WebAuthnSample.Services;

/// <summary>
/// Contains output for the <see cref="WebAuthnInteractionService.GenerateCredentialsCreateOptionsAsync"/> method.
/// </summary>
public class GenerateCredentialsCreateOptionsResult
{
    /// <summary>
    /// Gets error messages generated during the execution of the operation.
    /// </summary>
    public List<string> ErrorMessages { get; } = new();
    
    /// <summary>
    /// Gets whether the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Options))]
    public bool Succeeded => ErrorMessages.Count == 0;
    
    /// <summary>
    /// Gets the credential create options returned by the operation.
    /// </summary>
    
    public CredentialCreateOptions? Options { get; set; }
}