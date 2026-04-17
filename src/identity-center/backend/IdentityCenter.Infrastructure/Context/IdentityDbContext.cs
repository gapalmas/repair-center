using IdentityCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityCenter.Infrastructure.Context;

public sealed class IdentityDbContext : DbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("identity");

        // ── Rol ──────────────────────────────────────────────
        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("roles");
            e.HasKey(r => r.Id);
            e.Property(r => r.Nombre).HasMaxLength(80).IsRequired();
            e.Property(r => r.Descripcion).HasMaxLength(300);
            e.Property(r => r.Activo).IsRequired();
            e.HasIndex(r => r.Nombre).IsUnique();
        });

        // ── Tenant ───────────────────────────────────────────
        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("tenants");
            e.HasKey(t => t.Id);
            e.Property(t => t.Nombre).HasMaxLength(200).IsRequired();
            e.Property(t => t.EmailContacto).HasMaxLength(200).IsRequired();
            e.Property(t => t.Rfc).HasMaxLength(20);
            e.Property(t => t.Direccion).HasMaxLength(500);
            e.Property(t => t.Telefono).HasMaxLength(20);
            e.Property(t => t.Estado).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(t => t.Nombre).IsUnique();
        });

        // ── Usuario ──────────────────────────────────────────
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(u => u.Id);
            e.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
            e.Property(u => u.Apellido).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.DebeCambiarPassword).IsRequired();
            e.Property(u => u.Estado).HasConversion<string>().HasMaxLength(20);

            e.HasIndex(u => u.Email).IsUnique();

            e.HasOne(u => u.Tenant)
             .WithMany(t => t.Usuarios)
             .HasForeignKey(u => u.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

              e.HasOne(u => u.Rol)
               .WithMany(r => r.Usuarios)
               .HasForeignKey(u => u.RolId)
               .OnDelete(DeleteBehavior.Restrict);
        });

        // ── RefreshToken ─────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(r => r.Id);
            e.Property(r => r.Token).HasMaxLength(500).IsRequired();
            e.Property(r => r.DeviceId).HasMaxLength(200).IsRequired();
            e.Property(r => r.SessionFingerprint).HasMaxLength(128).IsRequired();
            e.Property(r => r.UserAgent).HasMaxLength(1024).IsRequired();

            e.HasIndex(r => r.Token).IsUnique();
            e.HasIndex(r => new { r.UsuarioId, r.SessionFingerprint });

            e.HasOne(r => r.Usuario)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
