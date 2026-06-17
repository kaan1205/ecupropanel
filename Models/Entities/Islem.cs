using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AraPanelWeb.Models.Entities;

public class Islem
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string AracPlaka { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string AracSahibi { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefon { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    [Range(0, 9999999)]
    public int AracKM { get; set; }

    [Required]
    public string YapilanIslem { get; set; } = string.Empty;

    public int EkleyenKullaniciId { get; set; }

    [ForeignKey(nameof(EkleyenKullaniciId))]
    public Kullanici EkleyenKullanici { get; set; } = null!;

    public DateTime EklenmeTarihi { get; set; } = DateTime.Now;

    public int? GuncelleyenId { get; set; }

    [ForeignKey(nameof(GuncelleyenId))]
    public Kullanici? GuncelleyenKullanici { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public ICollection<IslemFoto> Fotograflar { get; set; } = new List<IslemFoto>();
    public ICollection<IslemLog> Loglar { get; set; } = new List<IslemLog>();
}
