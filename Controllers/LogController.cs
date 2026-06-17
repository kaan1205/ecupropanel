using Microsoft.AspNetCore.Mvc;
using AraPanelWeb.Filters;
using Microsoft.EntityFrameworkCore;
using AraPanelWeb.Data;

namespace AraPanelWeb.Controllers;

[AdminRequired]
public class LogController : Controller
{
    private readonly AppDbContext _db;

    public LogController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? aksiyon, int? kullaniciId, DateTime? baslangic, DateTime? bitis)
    {
        var query = _db.IslemLoglari
            .Include(l => l.Kullanici)
            .Include(l => l.Islem)
            .AsQueryable();

        if (!string.IsNullOrEmpty(aksiyon))
            query = query.Where(l => l.Aksiyon == aksiyon);

        if (kullaniciId.HasValue)
            query = query.Where(l => l.KullaniciId == kullaniciId.Value);

        if (baslangic.HasValue)
            query = query.Where(l => l.LogTarihi >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(l => l.LogTarihi <= bitis.Value.AddDays(1));

        var loglar = await query
            .OrderByDescending(l => l.LogTarihi)
            .Take(500)
            .ToListAsync();

        ViewBag.Aksiyonlar = new[] { "GIRIS", "CIKIS", "KAYIT", "EKLE", "GUNCELLE", "SIL", "GORUNTULE" };
        ViewBag.Kullanicilar = await _db.Users
            .Where(k => k.AktifMi)
            .Select(k => new { k.Id, TamAd = k.Ad + " " + k.Soyad })
            .ToListAsync();

        return View(loglar);
    }
}
