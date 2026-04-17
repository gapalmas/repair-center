using IdentityCenter.Application.DTOs.Roles;
using IdentityCenter.Application.Interfaces;
using IdentityCenter.Domain.Entities;

namespace IdentityCenter.Application.Services;

public sealed class RolService
{
    private readonly IRolRepository _repo;
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IUnitOfWork _uow;

    public RolService(IRolRepository repo, IUsuarioRepository usuarioRepo, IUnitOfWork uow)
    {
        _repo = repo;
        _usuarioRepo = usuarioRepo;
        _uow = uow;
    }

    public async Task<RolDto> CrearAsync(CrearRolRequest request, CancellationToken ct)
    {
        await ValidarNombreDisponibleAsync(request.Nombre, null, ct);

        var rol = new Rol(request.Nombre.Trim(), LimpiarDescripcion(request.Descripcion));
        await _repo.AddAsync(rol, ct);
        await _uow.SaveChangesAsync(ct);

        return rol.ToDto();
    }

    public async Task<RolDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var rol = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Rol {id} no encontrado.");

        return rol.ToDto();
    }

    public async Task<List<RolDto>> GetAllAsync(bool soloActivos, CancellationToken ct)
    {
        var roles = await _repo.GetAllAsync(soloActivos, ct);
        return roles.Select(static rol => rol.ToDto()).ToList();
    }

    public async Task<RolDto> ActualizarAsync(Guid id, ActualizarRolRequest request, CancellationToken ct)
    {
        var rol = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Rol {id} no encontrado.");

        await ValidarNombreDisponibleAsync(request.Nombre, id, ct);

        rol.Actualizar(request.Nombre.Trim(), LimpiarDescripcion(request.Descripcion));
        _repo.Update(rol);
        await _uow.SaveChangesAsync(ct);

        return rol.ToDto();
    }

    public async Task DesactivarAsync(Guid id, CancellationToken ct)
    {
        var rol = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Rol {id} no encontrado.");

        rol.Desactivar();
        _repo.Update(rol);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ActivarAsync(Guid id, CancellationToken ct)
    {
        var rol = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Rol {id} no encontrado.");

        rol.Activar();
        _repo.Update(rol);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<bool> TieneUsuariosAsignadosAsync(Guid rolId, CancellationToken ct)
        => await _usuarioRepo.ExisteConRolAsync(rolId, ct);

    private async Task ValidarNombreDisponibleAsync(string nombre, Guid? rolIdActual, CancellationToken ct)
    {
        var nombreNormalizado = nombre.Trim();
        var existente = await _repo.GetByNombreAsync(nombreNormalizado, ct);

        if (existente is not null && existente.Id != rolIdActual)
            throw new InvalidOperationException($"Ya existe un rol con nombre {nombreNormalizado}.");
    }

    private static string? LimpiarDescripcion(string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            return null;

        return descripcion.Trim();
    }
}

internal static class RolMappingExtensions
{
    public static RolDto ToDto(this Rol rol) =>
        new(
            rol.Id,
            rol.Nombre,
            rol.Descripcion,
            rol.Activo,
            rol.Usuarios.Count,
            rol.CreadoEn,
            rol.ModificadoEn);
}