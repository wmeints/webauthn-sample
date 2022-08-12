using Fido2NetLib;

namespace WebAuthnSample.Services;

/// <summary>
/// Implements interaction logic for the WebAuthn flow in the application.
/// </summary>
public interface IWebAuthnInteractionService
{
    /// <summary>
    /// Generates options to create a new public key credential
    /// </summary>
    /// <param name="displayName">Display name to use for the user.</param>
    /// <param name="userName">User name to use for the user.</param>
    /// <returns>The options to use for creating a new public key credential.</returns>
    Task<GenerateCredentialsCreateOptionsResult> GenerateCredentialsCreateOptionsAsync(string displayName, string userName);

    /// <summary>
    /// Creates a new user with a public key credential attached to it.
    /// </summary>
    /// <param name="credentialCreateOptions">The options used for the registration process.</param>
    /// <param name="attestationResponse">The attestion response provided by the user in the browser.</param>
    /// <returns>An awaitable task with the outcome of the operation.</returns>
    Task<CreatePublicKeyCredentialResult> CreatePublicKeyCredentialAsync(
        CredentialCreateOptions credentialCreateOptions, AuthenticatorAttestationRawResponse attestationResponse);
}