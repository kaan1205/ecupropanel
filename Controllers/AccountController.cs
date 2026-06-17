using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

        if (await _userManager.IsInRoleAsync(kullanici, "Admin"))
            return RedirectToAction("Index", "Islem");

        return RedirectToAction("Beklemede");
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

        if (rol == "Admin")
        {
            TempData["Basari"] = "Kayıt başarılı! Hoş geldiniz.";
            return RedirectToAction("Index", "Islem");
        }

        return RedirectToAction("Beklemede");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Beklemede()
    {
        var kullanici = await _userManager.GetUserAsync(User);
        if (kullanici != null && await _userManager.IsInRoleAsync(kullanici, "Admin"))
            return RedirectToAction("Index", "Islem");

        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> RolKontrol()
    {
        var kullanici = await _userManager.GetUserAsync(User);
        if (kullanici != null && await _userManager.IsInRoleAsync(kullanici, "Admin"))
            return Json(new { admin = true });

        return Json(new { admin = false });
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
