using IdentityCenter.Domain.Common;

namespace IdentityCenter.Domain.Entities;

public sealed class Rol : AuditableEntity
{
    public string Nombre { get; private set; } = null!;
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; } = true;

    private readonly List<Usuario> _usuarios = [];
    public IReadOnlyCollection<Usuario> Usuarios => _usuarios.AsReadOnly();

    private Rol() { }

    public Rol(string nombre, string? descripcion = null)
    {
        Nombre = nombre;
        Descripcion = descripcion;
    }

    public void Actualizar(string nombre, string? descripcion)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        ModificadoEn = DateTime.UtcNow;
    }

    public void Desactivar()
    {
        Activo = false;
        ModificadoEn = DateTime.UtcNow;
    }

    public void Activar()
    {
        Activo = true;
        ModificadoEn = DateTime.UtcNow;
    }
}