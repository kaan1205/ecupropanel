using Microsoft.EntityFrameworkCore;
using AraPanelWeb.Data;
using AraPanelWeb.Models.Entities;
using AraPanelWeb.Models.ViewModels;

namespace AraPanelWeb.Services;

public class IslemService
{
    private readonly AppDbContext _db;
    private readonly FileService _fileService;
    private readonly LogService _logService;

    public IslemService(AppDbContext db, FileService fileService, LogService logService)
    {
        _db = db;
        _fileService = fileService;
        _logService = logService;
    }

    public async Task<IslemListeVM> Listele(IslemListeVM filtre)
    {
        var query = _db.Islemler
            .Include(i => i.EkleyenKullanici)
            .Include(i => i.Fotograflar)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtre.Plaka))
            query = query.Where(i => i.AracPlaka.Contains(filtre.Plaka));

        if (!string.IsNullOrWhiteSpace(filtre.AracSahibi))
            query = query.Where(i => i.AracSahibi.Contains(filtre.AracSahibi));

        if (filtre.BaslangicTarih.HasValue)
            query = query.Where(i => i.EklenmeTarihi >= filtre.BaslangicTarih.Value);

        if (filtre.BitisTarih.HasValue)
            query = query.Where(i => i.EklenmeTarihi <= filtre.BitisTarih.Value.AddDays(1));

        if (filtre.EkleyenKullaniciId.HasValue)
            query = query.Where(i => i.EkleyenKullaniciId == filtre.EkleyenKullaniciId.Value);

        if (filtre.MinKM.HasValue)
            query = query.Where(i => i.AracKM >= filtre.MinKM.Value);

        if (filtre.MaxKM.HasValue)
            query = query.Where(i => i.AracKM <= filtre.MaxKM.Value);

        var islemler = await query
            .OrderByDescending(i => i.EklenmeTarihi)
            .Select(i => new IslemSatirVM
            {
                Id = i.Id,
                AracPlaka = i.AracPlaka,
                AracSahibi = i.AracSahibi,
                AracKM = i.AracKM,
                YapilanIslem = i.YapilanIslem,
                EkleyenAd = i.EkleyenKullanici.Ad + " " + i.EkleyenKullanici.Soyad,
                EklenmeTarihi = i.EklenmeTarihi,
                FotoSayisi = i.Fotograflar.Count
            })
            .ToListAsync();

        var kullanicilar = await _db.Users
            .Where(k => k.AktifMi)
            .Select(k => new KullaniciSecVM
            {
                Id = k.Id,
                TamAd = k.Ad + " " + k.Soyad
            })
            .ToListAsync();

        filtre.Islemler = islemler;
        filtre.KullaniciListesi = kullanicilar;
        return filtre;
    }

    public async Task<int> Ekle(IslemEkleVM vm, int kullaniciId)
    {
        var islem = new Islem
        {
            AracPlaka = vm.AracPlaka.ToUpperInvariant().Trim(),
            AracSahibi = vm.AracSahibi.Trim(),
            AracKM = vm.AracKM,
            YapilanIslem = vm.YapilanIslem.Trim(),
            EkleyenKullaniciId = kullaniciId,
            EklenmeTarihi = DateTime.Now
        };

        _db.Islemler.Add(islem);
        await _db.SaveChangesAsync();

        if (vm.Fotograflar?.Count > 0)
        {
            var kaydedilenler = await _fileService.FotograflariKaydet(vm.Fotograflar, islem.Id);
            foreach (var (yol, orijinalAd) in kaydedilenler)
            {
                _db.IslemFotolari.Add(new IslemFoto
                {
                    IslemId = islem.Id,
                    DosyaYolu = yol,
                    OrijinalAd = orijinalAd,
                    YuklemeTarihi = DateTime.Now
                });
            }
            await _db.SaveChangesAsync();
        }

        await _logService.LogEkle("EKLE", islem.Id);
        return islem.Id;
    }

    public async Task<Islem?> Detay(int id)
    {
        return await _db.Islemler
            .Include(i => i.EkleyenKullanici)
            .Include(i => i.GuncelleyenKullanici)
            .Include(i => i.Fotograflar)
            .Include(i => i.Loglar)
                .ThenInclude(l => l.Kullanici)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<bool> Sil(int id)
    {
        var islem = await _db.Islemler.FindAsync(id);
        if (islem == null) return false;

        await _logService.LogEkle("SIL", id);
        _fileService.FotograflariSil(id);
        _db.Islemler.Remove(islem);
        await _db.SaveChangesAsync();
        return true;
    }
}
