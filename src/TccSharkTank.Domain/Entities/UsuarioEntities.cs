namespace TccSharkTank.Domain.Entities;

public sealed class UsuCargo
{
    public int Id { get; set; }
    public required string Nome { get; set; }

    public ICollection<UsuUsuario> Usuarios { get; set; } = new List<UsuUsuario>();
}

public sealed class UsuUsuario
{
    public long Id { get; set; }
    public required string Cpf { get; set; }
    public required string Email { get; set; }
    public required string Telefone { get; set; }
    public required string Senha { get; set; }
    public int CargoId { get; set; }
    public bool Status { get; set; } = true;
    public DateTime? UltimoLogin { get; set; }
    public required string Nome { get; set; }
    public required string Sobrenome { get; set; }

    public UsuCargo? Cargo { get; set; }
    public UsuPerfil? Perfil { get; set; }

    public ICollection<IdaIdeia> Ideias { get; set; } = new List<IdaIdeia>();
    public ICollection<PrpProposta> Propostas { get; set; } = new List<PrpProposta>();
    public ICollection<NtfNotificacao> NotificacoesRecebidas { get; set; } = new List<NtfNotificacao>();
    public ICollection<TrnLog> Logs { get; set; } = new List<TrnLog>();
}

public sealed class UsuPerfil : AuditableEntityBase<long>
{
    public long UsuarioId { get; set; }
    public string? Descricao { get; set; }
    public string? Cep { get; set; }
    public DateTime? DataNasc { get; set; }
    public string? LinkRedes { get; set; }

    public UsuUsuario? Usuario { get; set; }
}
