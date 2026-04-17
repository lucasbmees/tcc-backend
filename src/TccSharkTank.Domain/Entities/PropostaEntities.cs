namespace TccSharkTank.Domain.Entities;

public sealed class PrpAceite
{
    public int Id { get; set; }
    public required string Nome { get; set; }

    public ICollection<PrpInfo> Infos { get; set; } = new List<PrpInfo>();
}

public sealed class PrpProposta
{
    public long Id { get; set; }
    public long IdeiaId { get; set; }
    public long UsuarioId { get; set; }
    public bool Status { get; set; } = true;

    public IdaIdeia? Ideia { get; set; }
    public UsuUsuario? Usuario { get; set; }
    public ICollection<PrpInfo> Infos { get; set; } = new List<PrpInfo>();
}

public sealed class PrpInfo : AuditableEntityBase<long>
{
    public long PropostaId { get; set; }
    public string? Mensagem { get; set; }
    public decimal Valor { get; set; }
    public decimal FatiaPret { get; set; }
    public int AceiteId { get; set; }
    public string? Retorno { get; set; }

    public PrpProposta? Proposta { get; set; }
    public PrpAceite? Aceite { get; set; }
}
