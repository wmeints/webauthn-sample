using Fido2NetLib;
using Fido2NetLib.Objects;

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
    /// <returns>An awaitable task with the outcome of the operation.</returns>
    Task<GenerateCredentialsCreateOptionsResult> GenerateCredentialsCreateOptionsAsync(string displayName,
        string userName);

    /// <summary>
    /// Creates a new user with a public key credential attached to it.
    /// </summary>
    /// <param name="credentialCreateOptions">The options used for the registration process.</param>
    /// <param name="attestationResponse">The attestion response provided by the user in the browser.</param>
    /// <returns>An awaitable task with the outcome of the operation.</returns>
    Task<CreatePublicKeyCredentialResult> CreatePublicKeyCredentialAsync(
        CredentialCreateOptions credentialCreateOptions, AuthenticatorAttestationRawResponse attestationResponse);

    /// <summary>
    /// Generates options used to authenticate a user using an existing public key credential.
    /// </summary>
    /// <param name="userName">The user name of the user.</param>
    /// <param name="userVerification">The user verification requirement setting.</param>
    /// <returns>An awaitable task with the outcome of the operation.</returns>
    Task<GenerateAssertionOptionsResult> GenerateAssertionOptionsAsync(string userName,
        UserVerificationRequirement userVerification);

    /// <summary>
    /// Verifies an assertion response made against a previously generated set of assertion options. 
    /// </summary>
    /// <param name="assertionOptions">Assertion options to verify against.</param>
    /// <param name="assertionRawResponse">Assertion response received from the browser.</param>
    /// <returns>An awaitable task with the outcome of the operation.</returns>
    Task<AssertPublicKeyCredentialResult> AssertPublicKeyCredentialAsync(AssertionOptions assertionOptions,
        AuthenticatorAssertionRawResponse assertionRawResponse);
}