using IdentityCenter.Application.Interfaces;
using IdentityCenter.Domain.Entities;
using IdentityCenter.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityCenter.Infrastructure.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly IdentityDbContext _db;
    public TenantRepository(IdentityDbContext db) => _db = db;

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<List<Tenant>> GetAllAsync(CancellationToken ct = default)
        => await _db.Tenants.OrderBy(t => t.Nombre).ToListAsync(ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
        => await _db.Tenants.AddAsync(tenant, ct);

    public void Update(Tenant tenant)
        => _db.Tenants.Update(tenant);
}
