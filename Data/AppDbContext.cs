using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AraPanelWeb.Models.Entities;

namespace AraPanelWeb.Data;

public class AppDbContext : IdentityDbContext<Kullanici, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Islem> Islemler => Set<Islem>();
    public DbSet<IslemFoto> IslemFotolari => Set<IslemFoto>();
    public DbSet<IslemLog> IslemLoglari => Set<IslemLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Kullanici>(e =>
        {
            e.ToTable("Kullanicilar");
        });

        builder.Entity<IdentityRole<int>>(e =>
        {
            e.ToTable("Roller");
        });

        builder.Entity<IdentityUserRole<int>>(e => e.ToTable("KullaniciRolleri"));
        builder.Entity<IdentityUserClaim<int>>(e => e.ToTable("KullaniciClaimleri"));
        builder.Entity<IdentityUserLogin<int>>(e => e.ToTable("KullaniciGirisleri"));
        builder.Entity<IdentityUserToken<int>>(e => e.ToTable("KullaniciTokenlari"));
        builder.Entity<IdentityRoleClaim<int>>(e => e.ToTable("RolClaimleri"));

        builder.Entity<Islem>(e =>
        {
            e.ToTable("Islemler");
            e.HasOne(i => i.EkleyenKullanici)
                .WithMany(k => k.EklenenIslemler)
                .HasForeignKey(i => i.EkleyenKullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.GuncelleyenKullanici)
                .WithMany()
                .HasForeignKey(i => i.GuncelleyenId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<IslemFoto>(e =>
        {
            e.ToTable("IslemFotolari");
            e.HasOne(f => f.Islem)
                .WithMany(i => i.Fotograflar)
                .HasForeignKey(f => f.IslemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<IslemLog>(e =>
        {
            e.ToTable("IslemLoglari");
            e.HasOne(l => l.Islem)
                .WithMany(i => i.Loglar)
                .HasForeignKey(l => l.IslemId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(l => l.Kullanici)
                .WithMany(k => k.Loglar)
                .HasForeignKey(l => l.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
