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

public sealed record RecuperarSenhaRequest(string Email);

public sealed record RecuperarSenhaResponse(string Mensagem, string? Token);

public sealed record RedefinirSenhaRequest(string Token, string NovaSenha);

public sealed record MensagemResponseSimples(string Mensagem);

public sealed record PlanoInfoResponse(
    int Id,
    string Nome,
    string Codigo
);

public sealed record PlanoMeuResponse(
    PlanoInfoResponse Plano,
    List<string> Regalias
);

public sealed record AssinarPlanoRequest(string PlanoCodigo);

public sealed record AssinarPlanoResponse(
    string Mensagem,
    PlanoInfoResponse Plano,
    string Token
);

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
    string? LinkRedes,
    decimal? InvestTicketMin,
    decimal? InvestTicketMax,
    string? InvestInteresses,
    bool? ReceberEmailPropostas,
    bool? ReceberEmailMensagens,
    bool? ReceberEmailAlertas
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
    decimal? InvestTicketMin,
    decimal? InvestTicketMax,
    string? InvestInteresses,
    bool ReceberEmailPropostas,
    bool ReceberEmailMensagens,
    bool ReceberEmailAlertas,
    DateTime CreateDate,
    DateTime UpdateDate
);

public sealed record CreateIdeiaRequest(
    int CategoriaId,
    int EstagioId,
    string Nome,
    string? Regiao,
    string Cnpj,
    string? Descricao,
    string? LinkVideo,
    string? Imagem,
    decimal Fatia,
    decimal ValorCaptacao
);

public sealed record UpdateIdeiaRequest(
    int? CategoriaId,
    int? EstagioId,
    string? Nome,
    string? Regiao,
    string? Cnpj,
    string? Descricao,
    string? LinkVideo,
    string? Imagem,
    decimal? Fatia,
    decimal? ValorCaptacao
);

public sealed record IdeiaDetailsResponse(
    long IdaId,
    long IdaUsuarioId,
    string IdaNome,
    string? Regiao,
    int IdaCategoriaId,
    string CategoriaNome,
    int IdaEstagioId,
    string EstagioNome,
    int IdaStatusId,
    string StatusNome,
    string? IdaMotivoStatus,
    IdeiaInfoResponse? Info,
    List<IdeiaDocumentoResponse> Documentos,
    List<ComentarioResponse> Comentarios
);

public sealed record ComentarioResponse(
    long Id,
    long UsuarioId,
    string UsuarioNome,
    long? ParentId,
    string Texto,
    DateTime CreateDate,
    List<ComentarioResponse> Replies
);

public sealed record CreateComentarioRequest(
    string Texto,
    long? ParentId = null
);

public sealed record ConversaResponse(
    long Id,
    long OutroUsuarioId,
    string OutroUsuarioNome,
    long? IdeiaId,
    string? IdeiaNome,
    DateTime UpdateDate,
    MensagemResponse? UltimaMensagem
);

public sealed record MensagemResponse(
    long Id,
    long RemetenteId,
    string RemetenteNome,
    string Texto,
    bool Lida,
    DateTime CreateDate
);

public sealed record CreateMensagemRequest(
    long? ParaUsuarioId, // Usado para iniciar conversa
    long? IdeiaId,       // Opcional: contexto da conversa
    string Texto
);

public sealed record IdeiaInfoResponse(
    string IdaInfoCnpj,
    string? IdaInfoDescricao,
    string? IdaInfoLinkVideo,
    string? IdaInfoImagem,
    decimal IdaInfoFatia,
    decimal IdaInfoValorCaptacao,
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
    List<PropostaInfoResponse> Infos,
    string? InvestidorPlanoCodigo = null,
    string? InvestidorPlanoNome = null
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
