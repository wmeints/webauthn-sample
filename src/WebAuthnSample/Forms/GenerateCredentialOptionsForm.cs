using System.ComponentModel.DataAnnotations;

namespace WebAuthnSample.Forms;

public class GenerateCredentialOptionsForm
{
    [Required] 
    public string UserName { get; set; } = "";
    public string DisplayName { get; set; } = "";
}