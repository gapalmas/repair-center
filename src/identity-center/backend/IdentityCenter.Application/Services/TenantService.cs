using IdentityCenter.Application.DTOs.Tenants;
using IdentityCenter.Application.Interfaces;
using IdentityCenter.Domain.Entities;

namespace IdentityCenter.Application.Services;

public sealed class TenantService
{
    private readonly ITenantRepository _repo;
    private readonly IUnitOfWork _uow;

    public TenantService(ITenantRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<TenantDto> CrearAsync(CrearTenantRequest request, CancellationToken ct)
    {
        var tenant = new Tenant(request.Nombre, request.EmailContacto,
                                request.Rfc, request.Direccion, request.Telefono);

        await _repo.AddAsync(tenant, ct);
        await _uow.SaveChangesAsync(ct);

        return tenant.ToDto();
    }

    public async Task<TenantDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var tenant = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Tenant {id} no encontrado.");

        return tenant.ToDto();
    }

    public async Task<List<TenantDto>> GetAllAsync(CancellationToken ct)
    {
        var tenants = await _repo.GetAllAsync(ct);
        return tenants.Select(t => t.ToDto()).ToList();
    }

    public async Task<TenantDto> ActualizarAsync(Guid id, ActualizarTenantRequest request, CancellationToken ct)
    {
        var tenant = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Tenant {id} no encontrado.");

        tenant.Actualizar(request.Nombre, request.EmailContacto,
                          request.Rfc, request.Direccion, request.Telefono);

        _repo.Update(tenant);
        await _uow.SaveChangesAsync(ct);

        return tenant.ToDto();
    }

    public async Task SuspenderAsync(Guid id, CancellationToken ct)
    {
        var tenant = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Tenant {id} no encontrado.");

        tenant.Suspender();
        _repo.Update(tenant);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ActivarAsync(Guid id, CancellationToken ct)
    {
        var tenant = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Tenant {id} no encontrado.");

        tenant.Activar();
        _repo.Update(tenant);
        await _uow.SaveChangesAsync(ct);
    }
}

internal static class TenantMappingExtensions
{
    public static TenantDto ToDto(this Tenant tenant) =>
        new(tenant.Id, tenant.Nombre, tenant.EmailContacto,
            tenant.Rfc, tenant.Direccion, tenant.Telefono,
            tenant.Estado, tenant.CreadoEn);
}
