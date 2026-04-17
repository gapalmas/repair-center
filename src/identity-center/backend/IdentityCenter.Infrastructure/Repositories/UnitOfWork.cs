using IdentityCenter.Application.Interfaces;
using IdentityCenter.Infrastructure.Context;

namespace IdentityCenter.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IdentityDbContext _db;
    public UnitOfWork(IdentityDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
