namespace TccSharkTank.Application.Contracts;

public sealed record RegisterUserRequest(
    string Cpf,
    string Email,
    string Telefone,
    string Senha,
    string Nome,
    string Sobrenome,
    string CargoNome
);

public sealed record LoginRequest(string Email, string Senha);

public sealed record AuthResponse(long UsuarioId, string Cargo, string Token);

public sealed record UpdateUserRequest(
    string? Email,
    string? Telefone,
    string? Nome,
    string? Sobrenome,
    bool? Inativar,
    UpdatePerfilRequest? Perfil
);

public sealed record UpdatePerfilRequest(
    string? Descricao,
    string? Cep,
    DateTime? DataNasc,
    string? LinkRedes
);

public sealed record UserDetailsResponse(
    long UsuId,
    string UsuCpf,
    string UsuEmail,
    string UsuTelefone,
    bool UsuStatus,
    DateTime? UsuUltimoLogin,
    string UsuNome,
    string UsuSobrenome,
    string Cargo,
    PerfilResponse? Perfil
);

public sealed record PerfilResponse(
    string? Descricao,
    string? Cep,
    DateTime? DataNasc,
    string? LinkRedes,
    DateTime CreateDate,
    DateTime UpdateDate
);

public sealed record CreateIdeiaRequest(
    int CategoriaId,
    string Nome,
    string Cnpj,
    string? Descricao,
    string? LinkVideo,
    string? Imagem,
    decimal Fatia
);

public sealed record UpdateIdeiaRequest(
    int? CategoriaId,
    string? Nome,
    string? Cnpj,
    string? Descricao,
    string? LinkVideo,
    string? Imagem,
    decimal? Fatia
);

public sealed record IdeiaDetailsResponse(
    long IdaId,
    long IdaUsuarioId,
    string IdaNome,
    int IdaCategoriaId,
    string CategoriaNome,
    int IdaStatusId,
    string StatusNome,
    string? IdaMotivoStatus,
    IdeiaInfoResponse? Info,
    List<IdeiaDocumentoResponse> Documentos
);

public sealed record IdeiaInfoResponse(
    string IdaInfoCnpj,
    string? IdaInfoDescricao,
    string? IdaInfoLinkVideo,
    string? IdaInfoImagem,
    decimal IdaInfoFatia,
    DateTime CreateDate,
    DateTime UpdateDate
);

public sealed record IdeiaDocumentoResponse(long IdaDocumentoId, string Arquivo);

public sealed record ChangeIdeiaStatusRequest(int StatusId, string? Motivo);

public sealed record CreatePropostaRequest(string? Mensagem, decimal Valor, decimal FatiaPret);

public sealed record ResponderPropostaRequest(int AceiteId, string? Retorno);

public sealed record PropostaResponse(
    long PrpId,
    long PrpIdeiaId,
    long PrpUsuarioId,
    bool PrpStatus,
    List<PropostaInfoResponse> Infos
);

public sealed record PropostaInfoResponse(
    string? Mensagem,
    decimal Valor,
    decimal FatiaPret,
    int AceiteId,
    string AceiteNome,
    string? Retorno,
    DateTime CreateDate,
    DateTime UpdateDate
);

public sealed record DispararNotificacaoRequest(long UsuarioId, int TipoId, string Mensagem);

public sealed record NotificacaoResponse(long NtfId, int TipoId, string TipoNome, string Mensagem, bool Lida, DateTime CreateDate);

