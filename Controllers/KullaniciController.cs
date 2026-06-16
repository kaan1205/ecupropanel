using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AraPanelWeb.Data;
using AraPanelWeb.Models.Entities;
using AraPanelWeb.Models.ViewModels;
using AraPanelWeb.Services;

namespace AraPanelWeb.Controllers;

[Authorize(Roles = "Admin")]
public class KullaniciController : Controller
{
    private readonly UserManager<Kullanici> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly AppDbContext _db;
    private readonly LogService _logService;

    public KullaniciController(
        UserManager<Kullanici> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        AppDbContext db,
        LogService logService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var kullanicilar = await _userManager.Users
            .OrderBy(k => k.Ad)
            .ToListAsync();

        var islemSayilari = await _db.Islemler
            .GroupBy(i => i.EkleyenKullaniciId)
            .Select(g => new { KullaniciId = g.Key, Sayi = g.Count() })
            .ToDictionaryAsync(x => x.KullaniciId, x => x.Sayi);

        var model = new List<KullaniciListeItemVM>();
        foreach (var k in kullanicilar)
        {
            var roller = await _userManager.GetRolesAsync(k);
            model.Add(new KullaniciListeItemVM
            {
                Id = k.Id,
                Ad = k.Ad,
                Soyad = k.Soyad,
                Email = k.Email ?? "",
                Rol = roller.FirstOrDefault() ?? "Yok",
                OlusturulmaTarihi = k.OlusturulmaTarihi,
                AktifMi = k.AktifMi,
                IslemSayisi = islemSayilari.GetValueOrDefault(k.Id, 0)
            });
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Ekle()
    {
        return View(new KullaniciEkleVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(KullaniciEkleVM model)
    {
        if (!ModelState.IsValid) return View(model);

        var kullanici = new Kullanici
        {
            UserName = model.Email,
            Email = model.Email,
            Ad = model.Ad.Trim(),
            Soyad = model.Soyad.Trim(),
            OlusturulmaTarihi = DateTime.Now
        };

        var sonuc = await _userManager.CreateAsync(kullanici, model.Sifre);
        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
                ModelState.AddModelError("", hata.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(kullanici, model.Rol);
        await _userManager.AddClaimAsync(kullanici, new Claim("Ad", kullanici.Ad));
        await _userManager.AddClaimAsync(kullanici, new Claim("Soyad", kullanici.Soyad));

        TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} kullanıcısı oluşturuldu.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var kullanici = await _userManager.FindByIdAsync(id.ToString());
        if (kullanici == null) return NotFound();

        var roller = await _userManager.GetRolesAsync(kullanici);

        var model = new KullaniciDuzenleVM
        {
            Id = kullanici.Id,
            Ad = kullanici.Ad,
            Soyad = kullanici.Soyad,
            Email = kullanici.Email ?? "",
            Rol = roller.FirstOrDefault() ?? "Kullanici",
            AktifMi = kullanici.AktifMi
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(KullaniciDuzenleVM model)
    {
        if (!ModelState.IsValid) return View(model);

        var kullanici = await _userManager.FindByIdAsync(model.Id.ToString());
        if (kullanici == null) return NotFound();

        kullanici.Ad = model.Ad.Trim();
        kullanici.Soyad = model.Soyad.Trim();
        kullanici.Email = model.Email;
        kullanici.UserName = model.Email;
        kullanici.NormalizedEmail = model.Email.ToUpperInvariant();
        kullanici.NormalizedUserName = model.Email.ToUpperInvariant();
        kullanici.AktifMi = model.AktifMi;

        var sonuc = await _userManager.UpdateAsync(kullanici);
        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
                ModelState.AddModelError("", hata.Description);
            return View(model);
        }

        var mevcutRoller = await _userManager.GetRolesAsync(kullanici);
        await _userManager.RemoveFromRolesAsync(kullanici, mevcutRoller);
        await _userManager.AddToRoleAsync(kullanici, model.Rol);

        var mevcutClaimler = await _userManager.GetClaimsAsync(kullanici);
        var adClaim = mevcutClaimler.FirstOrDefault(c => c.Type == "Ad");
        var soyadClaim = mevcutClaimler.FirstOrDefault(c => c.Type == "Soyad");
        if (adClaim != null) await _userManager.RemoveClaimAsync(kullanici, adClaim);
        if (soyadClaim != null) await _userManager.RemoveClaimAsync(kullanici, soyadClaim);
        await _userManager.AddClaimAsync(kullanici, new Claim("Ad", kullanici.Ad));
        await _userManager.AddClaimAsync(kullanici, new Claim("Soyad", kullanici.Soyad));

        TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} güncellendi.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> SifreSifirla(int id)
    {
        var kullanici = await _userManager.FindByIdAsync(id.ToString());
        if (kullanici == null) return NotFound();

        return View(new SifreSifirlaVM
        {
            KullaniciId = kullanici.Id,
            KullaniciAd = $"{kullanici.Ad} {kullanici.Soyad}"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SifreSifirla(SifreSifirlaVM model)
    {
        if (!ModelState.IsValid) return View(model);

        var kullanici = await _userManager.FindByIdAsync(model.KullaniciId.ToString());
        if (kullanici == null) return NotFound();

        model.KullaniciAd = $"{kullanici.Ad} {kullanici.Soyad}";

        var token = await _userManager.GeneratePasswordResetTokenAsync(kullanici);
        var sonuc = await _userManager.ResetPasswordAsync(kullanici, token, model.YeniSifre);

        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
                ModelState.AddModelError("", hata.Description);
            return View(model);
        }

        TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} kullanıcısının şifresi sıfırlandı.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumDegistir(int id)
    {
        var kullanici = await _userManager.FindByIdAsync(id.ToString());
        if (kullanici == null) return NotFound();

        kullanici.AktifMi = !kullanici.AktifMi;
        await _userManager.UpdateAsync(kullanici);

        var durum = kullanici.AktifMi ? "aktif" : "pasif";
        TempData["Basari"] = $"{kullanici.Ad} {kullanici.Soyad} {durum} yapıldı.";
        return RedirectToAction("Index");
    }
}
