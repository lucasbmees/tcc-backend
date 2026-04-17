namespace TccSharkTank.Domain.Entities;

public sealed class TrnTipo
{
    public int Id { get; set; }
    public required string Nome { get; set; }

    public ICollection<TrnLog> Logs { get; set; } = new List<TrnLog>();
}

public sealed class TrnLog
{
    public long Id { get; set; }
    public int TipoId { get; set; }
    public long? UsuarioId { get; set; }
    public long? IdeiaId { get; set; }
    public long? PropostaId { get; set; }
    public DateTime CreateDate { get; set; }
    public required string Descricao { get; set; }

    public TrnTipo? Tipo { get; set; }
    public UsuUsuario? Usuario { get; set; }
    public IdaIdeia? Ideia { get; set; }
    public PrpProposta? Proposta { get; set; }
}
