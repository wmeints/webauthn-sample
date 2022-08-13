using System.Diagnostics.CodeAnalysis;
using Fido2NetLib;

namespace WebAuthnSample.Services;

public class GenerateAssertionOptionsResult
{
    public List<string> ErrorMessages { get; } = new();
    [MemberNotNullWhen(true, nameof(Options))]
    public bool Succeeded => ErrorMessages.Count == 0;
    public AssertionOptions? Options { get; set; }
}