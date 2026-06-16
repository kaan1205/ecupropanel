using System.ComponentModel.DataAnnotations;

namespace AraPanelWeb.Models.ViewModels;

public class LoginVM
{
    [Required(ErrorMessage = "Email zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    public string Sifre { get; set; } = string.Empty;

    public bool BeniHatirla { get; set; }
}
