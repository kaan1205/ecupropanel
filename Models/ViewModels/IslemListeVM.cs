namespace AraPanelWeb.Models.ViewModels;

public class IslemListeVM
{
    public string? Plaka { get; set; }
    public string? AracSahibi { get; set; }
    public DateTime? BaslangicTarih { get; set; }
    public DateTime? BitisTarih { get; set; }
    public int? EkleyenKullaniciId { get; set; }
    public int? MinKM { get; set; }
    public int? MaxKM { get; set; }

    public List<IslemSatirVM> Islemler { get; set; } = new();
    public List<KullaniciSecVM> KullaniciListesi { get; set; } = new();
}

public class IslemSatirVM
{
    public int Id { get; set; }
    public string AracPlaka { get; set; } = string.Empty;
    public string AracSahibi { get; set; } = string.Empty;
    public int AracKM { get; set; }
    public string YapilanIslem { get; set; } = string.Empty;
    public string EkleyenAd { get; set; } = string.Empty;
    public DateTime EklenmeTarihi { get; set; }
    public int FotoSayisi { get; set; }
}

public class KullaniciSecVM
{
    public int Id { get; set; }
    public string TamAd { get; set; } = string.Empty;
}
