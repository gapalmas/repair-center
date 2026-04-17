namespace IdentityCenter.Application.DTOs.Roles;

public sealed record CrearRolRequest(
    string Nombre,
    string? Descripcion = null);

public sealed record ActualizarRolRequest(
    string Nombre,
    string? Descripcion = null);

public sealed record RolDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    bool Activo,
    int UsuariosAsignados,
    DateTime CreadoEn,
    DateTime? ModificadoEn);