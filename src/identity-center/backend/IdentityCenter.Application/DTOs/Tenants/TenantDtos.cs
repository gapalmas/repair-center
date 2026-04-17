using IdentityCenter.Domain.Enums;

namespace IdentityCenter.Application.DTOs.Tenants;

public sealed record CrearTenantRequest(
    string Nombre,
    string EmailContacto,
    string? Rfc = null,
    string? Direccion = null,
    string? Telefono = null);

public sealed record ActualizarTenantRequest(
    string Nombre,
    string EmailContacto,
    string? Rfc = null,
    string? Direccion = null,
    string? Telefono = null);

public sealed record TenantDto(
    Guid Id,
    string Nombre,
    string EmailContacto,
    string? Rfc,
    string? Direccion,
    string? Telefono,
    EstadoTenant Estado,
    DateTime CreadoEn);
