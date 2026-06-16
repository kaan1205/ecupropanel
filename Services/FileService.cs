namespace AraPanelWeb.Services;

public class FileService
{
    private readonly string _webRoot;
    private readonly long _maxBoyut;
    private readonly string[] _izinliUzantilar;

    public FileService(IWebHostEnvironment env, IConfiguration config)
    {
        _webRoot = env.WebRootPath;
        _maxBoyut = config.GetValue<long>("FotoAyarlari:MaxDosyaBoyutu", 5242880);
        _izinliUzantilar = config.GetSection("FotoAyarlari:IzinliUzantilar")
            .Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };
    }

    public async Task<List<(string DosyaYolu, string OrijinalAd)>> FotograflariKaydet(
        List<IFormFile> dosyalar, int islemId)
    {
        var klasor = Path.Combine(_webRoot, "uploads", $"islem_{islemId}");
        Directory.CreateDirectory(klasor);

        var sonuclar = new List<(string, string)>();

        foreach (var dosya in dosyalar)
        {
            if (dosya.Length == 0 || dosya.Length > _maxBoyut) continue;

            var uzanti = Path.GetExtension(dosya.FileName).ToLowerInvariant();
            if (!_izinliUzantilar.Contains(uzanti)) continue;

            var dosyaAdi = $"{Guid.NewGuid()}{uzanti}";
            var tamYol = Path.Combine(klasor, dosyaAdi);

            using var stream = File.Create(tamYol);
            await dosya.CopyToAsync(stream);

            var relativeYol = $"/uploads/islem_{islemId}/{dosyaAdi}";
            sonuclar.Add((relativeYol, dosya.FileName));
        }

        return sonuclar;
    }

    public void FotograflariSil(int islemId)
    {
        var klasor = Path.Combine(_webRoot, "uploads", $"islem_{islemId}");
        if (Directory.Exists(klasor))
            Directory.Delete(klasor, recursive: true);
    }
}
