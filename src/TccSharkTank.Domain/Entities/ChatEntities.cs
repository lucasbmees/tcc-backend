namespace TccSharkTank.Domain.Entities;

public sealed class ChtConversa : AuditableEntityBase<long>
{
    public long Usuario1Id { get; set; }
    public long Usuario2Id { get; set; }
    public long? IdeiaId { get; set; }

    public UsuUsuario? Usuario1 { get; set; }
    public UsuUsuario? Usuario2 { get; set; }
    public IdaIdeia? Ideia { get; set; }
    public ICollection<ChtMensagem> Mensagens { get; set; } = new List<ChtMensagem>();
}

public sealed class ChtMensagem : AuditableEntityBase<long>
{
    public long ConversaId { get; set; }
    public long RemetenteId { get; set; }
    public required string Texto { get; set; }
    public bool Lida { get; set; }

    public ChtConversa? Conversa { get; set; }
    public UsuUsuario? Remetente { get; set; }
}
