using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AraPanelWeb.Models.Entities;

public class IslemFoto
{
    public int Id { get; set; }

    public int IslemId { get; set; }

    [ForeignKey(nameof(IslemId))]
    public Islem Islem { get; set; } = null!;

    [Required, MaxLength(500)]
    public string DosyaYolu { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string OrijinalAd { get; set; } = string.Empty;

    public DateTime YuklemeTarihi { get; set; } = DateTime.Now;
}
