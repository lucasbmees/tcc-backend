using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IUsuarioRepository
{
    Task<UsuUsuario?> GetByIdAsync(long usuId, CancellationToken cancellationToken);
    Task<UsuUsuario?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<UsuUsuario?> GetByCpfAsync(string cpf, CancellationToken cancellationToken);
    Task<UsuUsuario?> GetByTelefoneAsync(string telefone, CancellationToken cancellationToken);
    Task<List<UsuUsuario>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(UsuUsuario usuario, CancellationToken cancellationToken);
    void Update(UsuUsuario usuario);
}

public interface ICargoRepository
{
    Task<UsuCargo?> GetByIdAsync(int cargoId, CancellationToken cancellationToken);
    Task<UsuCargo?> GetByNomeAsync(string nome, CancellationToken cancellationToken);
    Task<List<UsuCargo>> ListAsync(CancellationToken cancellationToken);
}

public interface IIdeiaRepository
{
    Task<IdaIdeia?> GetByIdAsync(long idaId, CancellationToken cancellationToken);
    Task<List<IdaIdeia>> ListAsync(
        string? termo,
        int? categoriaId,
        int? estagioId,
        string? regiao,
        decimal? valorMin,
        decimal? valorMax,
        bool? apenasComDocumentos,
        CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken);
    Task<int> CountAtivasByUsuarioAsync(long usuarioId, CancellationToken cancellationToken);
    Task AddAsync(IdaIdeia ideia, CancellationToken cancellationToken);
    void Update(IdaIdeia ideia);
    Task AddComentarioAsync(IdaComentario comentario, CancellationToken cancellationToken);
    Task<List<IdaComentario>> GetComentariosByIdeiaIdAsync(long ideiaId, CancellationToken cancellationToken);
}

public interface IChatRepository
{
    Task<ChtConversa?> GetConversaByIdAsync(long conversaId, CancellationToken cancellationToken);
    Task<ChtConversa?> GetConversaEntreUsuariosAsync(long usuario1Id, long usuario2Id, long? ideiaId, CancellationToken cancellationToken);
    Task<List<ChtConversa>> ListMinhasConversasAsync(long usuarioId, CancellationToken cancellationToken);
    Task AddConversaAsync(ChtConversa conversa, CancellationToken cancellationToken);
    Task AddMensagemAsync(ChtMensagem mensagem, CancellationToken cancellationToken);
    Task<List<ChtMensagem>> GetMensagensByConversaIdAsync(long conversaId, CancellationToken cancellationToken);
    void UpdateConversa(ChtConversa conversa);
}

public interface IPropostaRepository
{
    Task<PrpProposta?> GetByIdAsync(long prpId, CancellationToken cancellationToken);
    Task<List<PrpProposta>> ListByUsuarioAsync(long usuarioId, CancellationToken cancellationToken);
    Task<List<PrpProposta>> ListRecebidasAsync(long empreendedorId, CancellationToken cancellationToken);
    Task AddAsync(PrpProposta proposta, CancellationToken cancellationToken);
    void Update(PrpProposta proposta);
    Task<List<PrpProposta>> ListByIdeiaAsync(long ideiaId, CancellationToken cancellationToken);
    Task<List<PrpProposta>> ListTodasAsync(CancellationToken cancellationToken);
    Task<bool> HasPropostaAceitaEntreUsuariosAsync(long usuarioAId, long usuarioBId, long ideiaId, CancellationToken cancellationToken);
}

public interface INotificacaoRepository
{
    Task<NtfNotificacao?> GetByIdAsync(long ntfId, CancellationToken cancellationToken);
    Task<List<NtfNotificacao>> ListByUsuarioAsync(long usuarioId, CancellationToken cancellationToken);
    Task AddAsync(NtfNotificacao notificacao, CancellationToken cancellationToken);
    void Update(NtfNotificacao notificacao);
}

public interface IPagamentoRepository
{
    Task AddAsync(PgtPagamento pagamento, CancellationToken cancellationToken);
    Task<List<PgtPagamento>> ListByUsuarioAsync(long usuarioId, CancellationToken cancellationToken);
}

public interface IGovernancaRepository
{
    Task AddDenunciaAsync(GovDenuncia denuncia, CancellationToken cancellationToken);
    Task<List<GovDenuncia>> ListDenunciasAsync(CancellationToken cancellationToken);
    Task<GovDenuncia?> GetDenunciaByIdAsync(long id, CancellationToken cancellationToken);
    void UpdateDenuncia(GovDenuncia denuncia);
}

public interface ILogRepository
{
    Task AddAsync(TrnLog log, CancellationToken cancellationToken);
}

public interface ILookupRepository
{
    Task<IdaStatus?> GetIdeiaStatusByIdAsync(int statusId, CancellationToken cancellationToken);
    Task<IdaEstagio?> GetIdeiaEstagioByIdAsync(int estagioId, CancellationToken cancellationToken);
    Task<IdaCategoria?> GetIdeiaCategoriaByIdAsync(int categoriaId, CancellationToken cancellationToken);
    Task<PrpAceite?> GetPropostaAceiteByIdAsync(int aceiteId, CancellationToken cancellationToken);
    Task<NtfTipo?> GetNotificacaoTipoByIdAsync(int tipoId, CancellationToken cancellationToken);
    Task<TrnTipo?> GetLogTipoByIdAsync(int tipoId, CancellationToken cancellationToken);
}
