namespace TccSharkTank.Domain.Entities;

public sealed class NtfTipo
{
    public int Id { get; set; }
    public required string Nome { get; set; }

    public ICollection<NtfNotificacao> Notificacoes { get; set; } = new List<NtfNotificacao>();
}

public sealed class NtfNotificacao
{
    public long Id { get; set; }
    public long UsuarioId { get; set; }
    public int TipoId { get; set; }
    public required string Mensagem { get; set; }
    public bool Lida { get; set; }
    public DateTime CreateDate { get; set; }

    public UsuUsuario? Usuario { get; set; }
    public NtfTipo? Tipo { get; set; }
}
