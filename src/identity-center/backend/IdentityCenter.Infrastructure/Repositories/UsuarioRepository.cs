using IdentityCenter.Application.Interfaces;
using IdentityCenter.Domain.Entities;
using IdentityCenter.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityCenter.Infrastructure.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly IdentityDbContext _db;
    public UsuarioRepository(IdentityDbContext db) => _db = db;

    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<Usuario?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        => await _db.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(r => r.Token == refreshToken), ct);

    public async Task<List<Usuario>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.Apellido)
            .ToListAsync(ct);

    public async Task<bool> ExisteConRolAsync(Guid rolId, CancellationToken ct = default)
        => await _db.Usuarios.AnyAsync(u => u.RolId == rolId, ct);

    public async Task AddAsync(Usuario usuario, CancellationToken ct = default)
        => await _db.Usuarios.AddAsync(usuario, ct);

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default)
        => await _db.RefreshTokens.AddAsync(refreshToken, ct);

    public void Update(Usuario usuario)
    {
        var entry = _db.Entry(usuario);

        if (entry.State == EntityState.Detached)
        {
            _db.Attach(usuario);
            entry.State = EntityState.Modified;
        }
    }
}
