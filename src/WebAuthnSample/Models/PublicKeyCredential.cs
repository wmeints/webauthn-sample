using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Fido2NetLib.Objects;

namespace WebAuthnSample.Models;

/// <summary>
/// Represents a WebAuthn credential.
/// </summary>
public class PublicKeyCredential
{
    /// <summary>
    /// Unique identifier for the credential
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// The binary identifier for the credential
    /// </summary>
    public byte[]? UserId { get; set; }
    
    /// <summary>
    /// The public key bytes for the credential
    /// </summary>
    public byte[]? PublicKey { get; set; }
    
    /// <summary>
    /// The user handle
    /// </summary>
    public byte[]? UserHandle { get; set; }
    
    /// <summary>
    /// The signature counter
    /// </summary>
    public uint SignatureCounter { get; set; }
    
    /// <summary>
    /// The type of credential
    /// </summary>
    public string? CredentialType { get; set; }
    
    /// <summary>
    /// The date the credential was registered
    /// </summary>
    public DateTime RegistrationDate { get; set; }
    
    /// <summary>
    /// The guid identifying the attestation for the credential.
    /// </summary>
    public Guid AttestationGuid { get; set; }
    
    /// <summary>
    /// The associated application user
    /// </summary>
    public ApplicationUser User { get; set; }
    
    /// <summary>
    /// Deserializes the descriptor data for the credential
    /// </summary>
    [NotMapped]
    public PublicKeyCredentialDescriptor? Descriptor
    {
        get { return string.IsNullOrWhiteSpace(DescriptorJson) ? null : JsonSerializer.Deserialize<PublicKeyCredentialDescriptor>(DescriptorJson); }
        set { DescriptorJson = JsonSerializer.Serialize(value); }
    }

    /// <summary>
    /// The JSON data for the descriptor
    /// </summary>
    public virtual string? DescriptorJson { get; set; }
}