using System.ComponentModel.DataAnnotations;

namespace AraPanelWeb.Models.ViewModels;

public class IslemEkleVM
{
    [Required(ErrorMessage = "Plaka zorunludur.")]
    [MaxLength(20)]
    public string AracPlaka { get; set; } = string.Empty;

    [Required(ErrorMessage = "Araç sahibi zorunludur.")]
    [MaxLength(150)]
    public string AracSahibi { get; set; } = string.Empty;

    [Required(ErrorMessage = "KM zorunludur.")]
    [Range(0, 9999999, ErrorMessage = "KM 0-9999999 arasında olmalıdır.")]
    public int AracKM { get; set; }

    [Required(ErrorMessage = "Yapılan işlem zorunludur.")]
    public string YapilanIslem { get; set; } = string.Empty;

    public List<IFormFile>? Fotograflar { get; set; }
}
