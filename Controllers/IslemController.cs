using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AraPanelWeb.Helpers;
using AraPanelWeb.Models.ViewModels;
using AraPanelWeb.Services;

namespace AraPanelWeb.Controllers;

[Authorize]
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
    public async Task<IActionResult> Detay(int id)
    {
        var islem = await _islemService.Detay(id);
        if (islem == null) return NotFound();

        await _logService.LogEkle("GORUNTULE", id);
        return View(islem);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(IslemListeVM filtre)
    {
        var model = await _islemService.Listele(filtre);
        var sb = new StringBuilder();
        sb.AppendLine("Plaka;Araç Sahibi;Telefon;E-Posta;KM;Yapılan İşlem;Ekleyen;Tarih;Fotoğraf Sayısı");

        foreach (var i in model.Islemler)
        {
            sb.AppendLine($"\"{i.AracPlaka}\";\"{i.AracSahibi}\";\"{i.Telefon}\";\"{i.Email}\";{i.AracKM};\"{i.YapilanIslem.Replace("\"", "\"\"")}\";\" {i.EkleyenAd}\";{i.EklenmeTarihi:dd.MM.yyyy HH:mm};{i.FotoSayisi}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"EcuPro_Islemler_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(IslemListeVM filtre)
    {
        var model = await _islemService.Listele(filtre);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("İşlemler");

        var headers = new[] { "Plaka", "Araç Sahibi", "Telefon", "E-Posta", "KM", "Yapılan İşlem", "Ekleyen", "Tarih", "Fotoğraf Sayısı" };
        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cell(1, c + 1).Value = headers[c];
            ws.Cell(1, c + 1).Style.Font.Bold = true;
            ws.Cell(1, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a1a2e");
            ws.Cell(1, c + 1).Style.Font.FontColor = XLColor.FromHtml("#E5A100");
        }

        for (int r = 0; r < model.Islemler.Count; r++)
        {
            var i = model.Islemler[r];
            ws.Cell(r + 2, 1).Value = i.AracPlaka;
            ws.Cell(r + 2, 2).Value = i.AracSahibi;
            ws.Cell(r + 2, 3).Value = i.Telefon ?? "";
            ws.Cell(r + 2, 4).Value = i.Email ?? "";
            ws.Cell(r + 2, 5).Value = i.AracKM;
            ws.Cell(r + 2, 6).Value = i.YapilanIslem;
            ws.Cell(r + 2, 7).Value = i.EkleyenAd;
            ws.Cell(r + 2, 8).Value = i.EklenmeTarihi.ToString("dd.MM.yyyy HH:mm");
            ws.Cell(r + 2, 9).Value = i.FotoSayisi;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"EcuPro_Islemler_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
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
