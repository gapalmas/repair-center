using IdentityCenter.Domain.Enums;

namespace IdentityCenter.Application.DTOs.Usuarios;

public sealed record CrearUsuarioRequest(
    Guid TenantId,
    string Nombre,
    string Apellido,
    string Email,
    string Password,
    Guid RolId);

public sealed record ActualizarUsuarioRequest(
    string Nombre,
    string Apellido,
    Guid RolId);

public sealed record CambiarPasswordRequest(
    string PasswordActual,
    string NuevoPassword);

public sealed record UsuarioDto(
    Guid Id,
    Guid TenantId,
    Guid RolId,
    string RolNombre,
    bool RolActivo,
    string Nombre,
    string Apellido,
    string Email,
    EstadoUsuario Estado,
    DateTime CreadoEn);
