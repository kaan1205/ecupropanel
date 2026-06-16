using Microsoft.AspNetCore.Identity;

namespace AraPanelWeb.Models.Entities;

public class Kullanici : IdentityUser<int>
{
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    public bool AktifMi { get; set; } = true;

    public ICollection<Islem> EklenenIslemler { get; set; } = new List<Islem>();
    public ICollection<IslemLog> Loglar { get; set; } = new List<IslemLog>();
}
