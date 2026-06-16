using System.Security.Claims;
using System.Text.Json;
using AraPanelWeb.Data;
using AraPanelWeb.Models.Entities;

namespace AraPanelWeb.Services;

public class LogService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContext;

    public LogService(AppDbContext db, IHttpContextAccessor httpContext)
    {
        _db = db;
        _httpContext = httpContext;
    }

    public async Task LogEkle(string aksiyon, int? islemId = null, object? detay = null)
    {
        var context = _httpContext.HttpContext;
        if (context?.User.Identity?.IsAuthenticated != true) return;

        var kullaniciId = int.Parse(
            context.User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        _db.IslemLoglari.Add(new IslemLog
        {
            Aksiyon = aksiyon,
            IslemId = islemId,
            KullaniciId = kullaniciId,
            DetayJson = detay != null ? JsonSerializer.Serialize(detay) : null,
            LogTarihi = DateTime.Now,
            IPAdresi = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers.UserAgent.ToString()
        });
        await _db.SaveChangesAsync();
    }
}
