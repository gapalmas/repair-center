using IdentityCenter.Application.Interfaces;
using IdentityCenter.Domain.Entities;
using IdentityCenter.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityCenter.Infrastructure.Repositories;

public sealed class RolRepository : IRolRepository
{
    private readonly IdentityDbContext _db;

    public RolRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<Rol?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Roles
            .Include(r => r.Usuarios)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Rol?> GetByNombreAsync(string nombre, CancellationToken ct = default)
        => await _db.Roles
            .Include(r => r.Usuarios)
            .FirstOrDefaultAsync(r => r.Nombre.ToLower() == nombre.ToLower(), ct);

    public async Task<List<Rol>> GetAllAsync(bool soloActivos, CancellationToken ct = default)
    {
        var query = _db.Roles
            .Include(r => r.Usuarios)
            .AsQueryable();

        if (soloActivos)
            query = query.Where(r => r.Activo);

        return await query
            .OrderBy(r => r.Nombre)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Rol rol, CancellationToken ct = default)
        => await _db.Roles.AddAsync(rol, ct);

    public void Update(Rol rol)
    {
        var entry = _db.Entry(rol);

        if (entry.State == EntityState.Detached)
        {
            _db.Attach(rol);
            entry.State = EntityState.Modified;
        }
    }
}