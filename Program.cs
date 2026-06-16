using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AraPanelWeb.Data;
using AraPanelWeb.Models.Entities;
using AraPanelWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Veritabanı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var provider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";

if (provider == "SqlServer")
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=EcuProPanel.db"));
}

// Identity
builder.Services.AddIdentity<Kullanici, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Servisler
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<IslemService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// DB'yi otomatik oluştur (migration)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Islem}/{action=Index}/{id?}");

app.Run();
