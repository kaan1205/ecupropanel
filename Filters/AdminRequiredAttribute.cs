using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AraPanelWeb.Models.Entities;

namespace AraPanelWeb.Filters;

public class AdminRequiredFilter : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        var userManager = context.HttpContext.RequestServices
            .GetRequiredService<UserManager<Kullanici>>();

        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        var kullanici = await userManager.FindByIdAsync(userId);
        if (kullanici == null || !kullanici.AktifMi)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        var isAdmin = await userManager.IsInRoleAsync(kullanici, "Admin");
        if (!isAdmin)
        {
            context.Result = new RedirectToActionResult("Beklemede", "Account", null);
        }
    }
}

public class AdminRequiredAttribute : TypeFilterAttribute
{
    public AdminRequiredAttribute() : base(typeof(AdminRequiredFilter)) { }
}
