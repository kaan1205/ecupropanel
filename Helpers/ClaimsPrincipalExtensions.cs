using System.Security.Claims;

namespace AraPanelWeb.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int GetId(this ClaimsPrincipal user)
    {
        return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    public static string GetAd(this ClaimsPrincipal user)
    {
        return user.FindFirstValue("Ad") ?? string.Empty;
    }

    public static string GetTamAd(this ClaimsPrincipal user)
    {
        var ad = user.FindFirstValue("Ad") ?? "";
        var soyad = user.FindFirstValue("Soyad") ?? "";
        return $"{ad} {soyad}".Trim();
    }
}
