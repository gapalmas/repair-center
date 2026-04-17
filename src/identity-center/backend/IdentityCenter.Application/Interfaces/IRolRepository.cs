using IdentityCenter.Domain.Entities;

namespace IdentityCenter.Application.Interfaces;

public interface IRolRepository
{
    Task<Rol?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Rol?> GetByNombreAsync(string nombre, CancellationToken ct = default);
    Task<List<Rol>> GetAllAsync(bool soloActivos, CancellationToken ct = default);
    Task AddAsync(Rol rol, CancellationToken ct = default);
    void Update(Rol rol);
}