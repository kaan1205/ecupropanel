using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AraPanelWeb.Models.Entities;

public class IslemLog
{
    public int Id { get; set; }

    public int? IslemId { get; set; }

    [ForeignKey(nameof(IslemId))]
    public Islem? Islem { get; set; }

    public int KullaniciId { get; set; }

    [ForeignKey(nameof(KullaniciId))]
    public Kullanici Kullanici { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Aksiyon { get; set; } = string.Empty;

    public string? DetayJson { get; set; }

    public DateTime LogTarihi { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? IPAdresi { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }
}
