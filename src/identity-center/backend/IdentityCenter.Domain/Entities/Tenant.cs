using IdentityCenter.Domain.Common;
using IdentityCenter.Domain.Enums;

namespace IdentityCenter.Domain.Entities;

public sealed class Tenant : AuditableEntity
{
    public string Nombre { get; private set; } = null!;
    public string? Rfc { get; private set; }
    public string? Direccion { get; private set; }
    public string? Telefono { get; private set; }
    public string EmailContacto { get; private set; } = null!;
    public EstadoTenant Estado { get; private set; } = EstadoTenant.Activo;

    private readonly List<Usuario> _usuarios = [];
    public IReadOnlyCollection<Usuario> Usuarios => _usuarios.AsReadOnly();

    private Tenant() { }

    public Tenant(string nombre, string emailContacto, string? rfc = null,
                  string? direccion = null, string? telefono = null)
    {
        Nombre = nombre;
        EmailContacto = emailContacto;
        Rfc = rfc;
        Direccion = direccion;
        Telefono = telefono;
    }

    public void Actualizar(string nombre, string emailContacto, string? rfc,
                           string? direccion, string? telefono)
    {
        Nombre = nombre;
        EmailContacto = emailContacto;
        Rfc = rfc;
        Direccion = direccion;
        Telefono = telefono;
        ModificadoEn = DateTime.UtcNow;
    }

    public void Suspender()
    {
        Estado = EstadoTenant.Suspendido;
        ModificadoEn = DateTime.UtcNow;
    }

    public void Activar()
    {
        Estado = EstadoTenant.Activo;
        ModificadoEn = DateTime.UtcNow;
    }

    public void Desactivar()
    {
        Estado = EstadoTenant.Inactivo;
        ModificadoEn = DateTime.UtcNow;
    }
}
