namespace TccSharkTank.Domain.Entities;

public sealed class IdaStatus
{
    public int Id { get; set; }
    public required string Nome { get; set; }

    public ICollection<IdaIdeia> Ideias { get; set; } = new List<IdaIdeia>();
}

public sealed class IdaEstagio
{
    public int Id { get; set; }
    public required string Nome { get; set; }

    public ICollection<IdaIdeia> Ideias { get; set; } = new List<IdaIdeia>();
}

public sealed class IdaCategoria
{
    public int Id { get; set; }
    public required string Nome { get; set; }

    public ICollection<IdaIdeia> Ideias { get; set; } = new List<IdaIdeia>();
}

public sealed class IdaIdeia : AuditableEntityBase<long>
{
    public long UsuarioId { get; set; }
    public int StatusId { get; set; }
    public string? MotivoStatus { get; set; }
    public int CategoriaId { get; set; }
    public int EstagioId { get; set; }
    public required string Nome { get; set; }
    public string? Regiao { get; set; }

    public UsuUsuario? Usuario { get; set; }
    public IdaStatus? Status { get; set; }
    public IdaEstagio? Estagio { get; set; }
    public IdaCategoria? Categoria { get; set; }
    public IdaInfo? Info { get; set; }

    public ICollection<IdaDocumento> Documentos { get; set; } = new List<IdaDocumento>();
    public ICollection<PrpProposta> Propostas { get; set; } = new List<PrpProposta>();
    public ICollection<IdaComentario> Comentarios { get; set; } = new List<IdaComentario>();
}

public sealed class IdaComentario : AuditableEntityBase<long>
{
    public long IdeiaId { get; set; }
    public long UsuarioId { get; set; }
    public long? ParentId { get; set; } // Para respostas
    public required string Texto { get; set; }

    public IdaIdeia? Ideia { get; set; }
    public UsuUsuario? Usuario { get; set; }
    public IdaComentario? Parent { get; set; }
    public ICollection<IdaComentario> Replies { get; set; } = new List<IdaComentario>();
}

public sealed class IdaInfo : AuditableEntityBase<long>
{
    public long IdeiaId { get; set; }
    public required string Cnpj { get; set; }
    public string? Descricao { get; set; }
    public string? LinkVideo { get; set; }
    public string? Imagem { get; set; }
    public decimal Fatia { get; set; }
    public decimal ValorCaptacao { get; set; }

    public IdaIdeia? Ideia { get; set; }
}

public sealed class IdaDocumento
{
    public long Id { get; set; }
    public long IdeiaId { get; set; }
    public required string Arquivo { get; set; }

    public IdaIdeia? Ideia { get; set; }
}
