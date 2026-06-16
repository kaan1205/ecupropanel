using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AraPanelWeb.Models.Entities;
using AraPanelWeb.Models.ViewModels;
using AraPanelWeb.Services;

namespace AraPanelWeb.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<Kullanici> _userManager;
    private readonly SignInManager<Kullanici> _signInManager;
    private readonly LogService _logService;

    public AccountController(
        UserManager<Kullanici> userManager,
        SignInManager<Kullanici> signInManager,
        LogService logService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logService = logService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Islem");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var kullanici = await _userManager.FindByEmailAsync(model.Email);
        if (kullanici == null || !kullanici.AktifMi)
        {
            ModelState.AddModelError("", "Geçersiz email veya şifre.");
            return View(model);
        }

        var sonuc = await _signInManager.PasswordSignInAsync(
            kullanici, model.Sifre, model.BeniHatirla, lockoutOnFailure: false);

        if (!sonuc.Succeeded)
        {
            ModelState.AddModelError("", "Geçersiz email veya şifre.");
            return View(model);
        }

        await _logService.LogEkle("GIRIS");

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Islem");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Islem");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM model)
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

        await _userManager.AddClaimAsync(kullanici, new Claim("Ad", kullanici.Ad));
        await _userManager.AddClaimAsync(kullanici, new Claim("Soyad", kullanici.Soyad));

        var toplamKullanici = _userManager.Users.Count();
        var rol = toplamKullanici == 1 ? "Admin" : "Kullanici";
        await _userManager.AddToRoleAsync(kullanici, rol);

        await _signInManager.SignInAsync(kullanici, isPersistent: false);
        await _logService.LogEkle("KAYIT");

        TempData["Basari"] = "Kayıt başarılı! Hoş geldiniz.";
        return RedirectToAction("Index", "Islem");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _logService.LogEkle("CIKIS");
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
