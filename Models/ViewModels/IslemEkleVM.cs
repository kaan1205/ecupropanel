using System.ComponentModel.DataAnnotations;
using AraPanelWeb.Models.Entities;

namespace AraPanelWeb.Models.ViewModels;

public class IslemEkleVM
{
    [Required(ErrorMessage = "Plaka zorunludur.")]
    [MaxLength(20)]
    public string AracPlaka { get; set; } = string.Empty;

    [Required(ErrorMessage = "Araç sahibi zorunludur.")]
    [MaxLength(150)]
    public string AracSahibi { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [MaxLength(20)]
    public string? Telefon { get; set; }

    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [MaxLength(150)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "KM zorunludur.")]
    [Range(0, 9999999, ErrorMessage = "KM 0-9999999 arasında olmalıdır.")]
    public int AracKM { get; set; }

    [Required(ErrorMessage = "Yapılan işlem zorunludur.")]
    public string YapilanIslem { get; set; } = string.Empty;

    public List<IFormFile>? Fotograflar { get; set; }
}

public class IslemDuzenleVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Plaka zorunludur.")]
    [MaxLength(20)]
    public string AracPlaka { get; set; } = string.Empty;

    [Required(ErrorMessage = "Araç sahibi zorunludur.")]
    [MaxLength(150)]
    public string AracSahibi { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [MaxLength(20)]
    public string? Telefon { get; set; }

    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [MaxLength(150)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "KM zorunludur.")]
    [Range(0, 9999999, ErrorMessage = "KM 0-9999999 arasında olmalıdır.")]
    public int AracKM { get; set; }

    [Required(ErrorMessage = "Yapılan işlem zorunludur.")]
    public string YapilanIslem { get; set; } = string.Empty;

    public List<IFormFile>? YeniFotograflar { get; set; }

    public List<IslemFoto> MevcutFotograflar { get; set; } = new();
}
