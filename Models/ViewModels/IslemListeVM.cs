namespace AraPanelWeb.Models.ViewModels;

public class IslemListeVM
{
    public string? Arama { get; set; }
    public DateTime? BaslangicTarih { get; set; }
    public DateTime? BitisTarih { get; set; }

    public int Sayfa { get; set; } = 1;
    public int SayfaBoyutu { get; set; } = 10;
    public int ToplamKayit { get; set; }
    public int ToplamSayfa => (int)Math.Ceiling((double)ToplamKayit / SayfaBoyutu);

    public List<IslemSatirVM> Islemler { get; set; } = new();
    public List<KullaniciSecVM> KullaniciListesi { get; set; } = new();
}

public class IslemSatirVM
{
    public int Id { get; set; }
    public string AracPlaka { get; set; } = string.Empty;
    public string AracSahibi { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Email { get; set; }
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
