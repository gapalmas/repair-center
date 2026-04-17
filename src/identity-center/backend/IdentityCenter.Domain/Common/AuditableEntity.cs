namespace IdentityCenter.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime? ModificadoEn { get; set; }
}
