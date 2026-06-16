using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AraPanelWeb.Helpers;
using AraPanelWeb.Models.ViewModels;
using AraPanelWeb.Services;

namespace AraPanelWeb.Controllers;

[Authorize(Roles = "Admin")]
public class IslemController : Controller
{
    private readonly IslemService _islemService;
    private readonly LogService _logService;

    public IslemController(IslemService islemService, LogService logService)
    {
        _islemService = islemService;
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(IslemListeVM filtre)
    {
        var model = await _islemService.Listele(filtre);
        return View(model);
    }

    [HttpGet]
    public IActionResult Ekle()
    {
        return View(new IslemEkleVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(IslemEkleVM model)
    {
        if (!ModelState.IsValid) return View(model);

        var kullaniciId = User.GetId();
        var islemId = await _islemService.Ekle(model, kullaniciId);

        TempData["Basari"] = "İşlem başarıyla eklendi.";
        return RedirectToAction("Detay", new { id = islemId });
    }

    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var model = await _islemService.DuzenleDetay(id);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(IslemDuzenleVM model, List<int>? SilinenFotolar)
    {
        if (!ModelState.IsValid)
        {
            var detay = await _islemService.DuzenleDetay(model.Id);
            if (detay != null) model.MevcutFotograflar = detay.MevcutFotograflar;
            return View(model);
        }

        var kullaniciId = User.GetId();
        var sonuc = await _islemService.Guncelle(model, kullaniciId, SilinenFotolar);
        if (!sonuc) return NotFound();

        TempData["Basari"] = "İşlem başarıyla güncellendi.";
        return RedirectToAction("Detay", new { id = model.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Detay(int id)
    {
        var islem = await _islemService.Detay(id);
        if (islem == null) return NotFound();

        await _logService.LogEkle("GORUNTULE", id);
        return View(islem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(int id)
    {
        var sonuc = await _islemService.Sil(id);
        if (!sonuc) return NotFound();

        TempData["Basari"] = "İşlem silindi.";
        return RedirectToAction("Index");
    }
}
