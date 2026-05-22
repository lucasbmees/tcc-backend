using Microsoft.EntityFrameworkCore;
using TccSharkTank.Application.Abstractions.Persistence;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => _db.SaveChangesAsync(cancellationToken);
}

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _db;

    public UsuarioRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<UsuUsuario?> GetByIdAsync(long usuId, CancellationToken cancellationToken)
    {
        return _db.UsuUsuarios
            .Include(u => u.Cargo)
            .Include(u => u.Plano)
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Id == usuId, cancellationToken);
    }

    public Task<UsuUsuario?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _db.UsuUsuarios
            .Include(u => u.Cargo)
            .Include(u => u.Plano)
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<UsuUsuario?> GetByCpfAsync(string cpf, CancellationToken cancellationToken)
    {
        return _db.UsuUsuarios
            .Include(u => u.Cargo)
            .Include(u => u.Plano)
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Cpf == cpf, cancellationToken);
    }

    public Task<UsuUsuario?> GetByTelefoneAsync(string telefone, CancellationToken cancellationToken)
    {
        return _db.UsuUsuarios
            .Include(u => u.Cargo)
            .Include(u => u.Plano)
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Telefone == telefone, cancellationToken);
    }

    public Task<List<UsuUsuario>> ListAsync(CancellationToken cancellationToken)
    {
        return _db.UsuUsuarios
            .Include(u => u.Cargo)
            .Include(u => u.Plano)
            .Include(u => u.Perfil)
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(UsuUsuario usuario, CancellationToken cancellationToken) => _db.UsuUsuarios.AddAsync(usuario, cancellationToken).AsTask();

    public void Update(UsuUsuario usuario) => _db.UsuUsuarios.Update(usuario);
}

public sealed class CargoRepository : ICargoRepository
{
    private readonly AppDbContext _db;

    public CargoRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<UsuCargo?> GetByIdAsync(int cargoId, CancellationToken cancellationToken)
        => _db.UsuCargos.FirstOrDefaultAsync(c => c.Id == cargoId, cancellationToken);

    public Task<UsuCargo?> GetByNomeAsync(string nome, CancellationToken cancellationToken)
        => _db.UsuCargos.FirstOrDefaultAsync(c => c.Nome == nome, cancellationToken);

    public Task<List<UsuCargo>> ListAsync(CancellationToken cancellationToken)
        => _db.UsuCargos.OrderBy(c => c.Id).ToListAsync(cancellationToken);
}

public sealed class IdeiaRepository : IIdeiaRepository
{
    private readonly AppDbContext _db;

    public IdeiaRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<IdaIdeia?> GetByIdAsync(long idaId, CancellationToken cancellationToken)
    {
        return _db.IdaIdeias
            .Include(i => i.Status)
            .Include(i => i.Categoria)
            .Include(i => i.Estagio)
            .Include(i => i.Usuario).ThenInclude(u => u!.Plano)
            .Include(i => i.Info)
            .Include(i => i.Documentos)
            .FirstOrDefaultAsync(i => i.Id == idaId, cancellationToken);
    }

    public Task<List<IdaIdeia>> ListAsync(
        string? termo,
        int? categoriaId,
        int? estagioId,
        string? regiao,
        decimal? valorMin,
        decimal? valorMax,
        bool? apenasComDocumentos,
        CancellationToken cancellationToken)
    {
        var query = _db.IdaIdeias
            .Include(i => i.Status)
            .Include(i => i.Categoria)
            .Include(i => i.Estagio)
            .Include(i => i.Usuario).ThenInclude(u => u!.Plano)
            .Include(i => i.Info)
            .Include(i => i.Documentos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var lowerTermo = termo.ToLower();
            query = query.Where(i => i.Nome.ToLower().Contains(lowerTermo) || 
                                     (i.Info != null && i.Info.Descricao != null && i.Info.Descricao.ToLower().Contains(lowerTermo)));
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(i => i.CategoriaId == categoriaId.Value);
        }

        if (estagioId.HasValue)
        {
            query = query.Where(i => i.EstagioId == estagioId.Value);
        }

        if (!string.IsNullOrWhiteSpace(regiao))
        {
            var lowerRegiao = regiao.ToLower();
            query = query.Where(i => i.Regiao != null && i.Regiao.ToLower().Contains(lowerRegiao));
        }

        if (valorMin.HasValue)
        {
            query = query.Where(i => i.Info != null && i.Info.ValorCaptacao >= valorMin.Value);
        }

        if (valorMax.HasValue)
        {
            query = query.Where(i => i.Info != null && i.Info.ValorCaptacao <= valorMax.Value);
        }

        if (apenasComDocumentos == true)
        {
            query = query.Where(i => i.Documentos.Any());
        }

        return query
            .OrderByDescending(i => i.Usuario != null ? i.Usuario.PlanoId : 0)
            .ThenByDescending(i => i.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken) => _db.IdaIdeias.CountAsync(cancellationToken);

    public Task<int> CountAtivasByUsuarioAsync(long usuarioId, CancellationToken cancellationToken)
        => _db.IdaIdeias.CountAsync(i => i.UsuarioId == usuarioId && (i.StatusId == 1 || i.StatusId == 2), cancellationToken);

    public Task AddAsync(IdaIdeia ideia, CancellationToken cancellationToken) => _db.IdaIdeias.AddAsync(ideia, cancellationToken).AsTask();

    public void Update(IdaIdeia ideia) => _db.IdaIdeias.Update(ideia);

    public async Task AddComentarioAsync(IdaComentario comentario, CancellationToken cancellationToken)
    {
        await _db.IdaComentarios.AddAsync(comentario, cancellationToken);
    }

    public async Task<List<IdaComentario>> GetComentariosByIdeiaIdAsync(long ideiaId, CancellationToken cancellationToken)
    {
        return await _db.IdaComentarios
            .Include(c => c.Usuario)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Usuario)
            .Where(c => c.IdeiaId == ideiaId && c.ParentId == null)
            .OrderByDescending(c => c.CreateDate)
            .ToListAsync(cancellationToken);
    }
}

public sealed class EfChatRepository : IChatRepository
{
    private readonly AppDbContext _db;

    public EfChatRepository(AppDbContext db) => _db = db;

    public Task<ChtConversa?> GetConversaByIdAsync(long conversaId, CancellationToken cancellationToken)
    {
        return _db.ChtConversas
            .Include(c => c.Usuario1)
            .Include(c => c.Usuario2)
            .Include(c => c.Ideia)
            .FirstOrDefaultAsync(c => c.Id == conversaId, cancellationToken);
    }

    public Task<ChtConversa?> GetConversaEntreUsuariosAsync(long usuario1Id, long usuario2Id, long? ideiaId, CancellationToken cancellationToken)
    {
        return _db.ChtConversas
            .FirstOrDefaultAsync(c =>
                ((c.Usuario1Id == usuario1Id && c.Usuario2Id == usuario2Id) ||
                 (c.Usuario1Id == usuario2Id && c.Usuario2Id == usuario1Id)) &&
                c.IdeiaId == ideiaId, cancellationToken);
    }

    public Task<List<ChtConversa>> ListMinhasConversasAsync(long usuarioId, CancellationToken cancellationToken)
    {
        return _db.ChtConversas
            .Include(c => c.Usuario1)
            .Include(c => c.Usuario2)
            .Include(c => c.Ideia)
            .Where(c => c.Usuario1Id == usuarioId || c.Usuario2Id == usuarioId)
            .OrderByDescending(c => c.UpdateDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddConversaAsync(ChtConversa conversa, CancellationToken cancellationToken)
    {
        await _db.ChtConversas.AddAsync(conversa, cancellationToken);
    }

    public async Task AddMensagemAsync(ChtMensagem mensagem, CancellationToken cancellationToken)
    {
        await _db.ChtMensagens.AddAsync(mensagem, cancellationToken);
    }

    public Task<List<ChtMensagem>> GetMensagensByConversaIdAsync(long conversaId, CancellationToken cancellationToken)
    {
        return _db.ChtMensagens
            .Where(m => m.ConversaId == conversaId)
            .OrderBy(m => m.CreateDate)
            .ToListAsync(cancellationToken);
    }

    public void UpdateConversa(ChtConversa conversa) => _db.ChtConversas.Update(conversa);
}

public sealed class EfPropostaRepository : IPropostaRepository
{
    private readonly AppDbContext _db;

    public EfPropostaRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<PrpProposta?> GetByIdAsync(long prpId, CancellationToken cancellationToken)
    {
        return _db.PrpPropostas
            .Include(p => p.Ideia)
            .Include(p => p.Usuario).ThenInclude(u => u!.Plano)
            .Include(p => p.Infos).ThenInclude(i => i.Aceite)
            .FirstOrDefaultAsync(p => p.Id == prpId, cancellationToken);
    }

    public Task<List<PrpProposta>> ListByUsuarioAsync(long usuarioId, CancellationToken cancellationToken)
    {
        return _db.PrpPropostas
            .Include(p => p.Ideia)
            .Include(p => p.Usuario).ThenInclude(u => u!.Plano)
            .Include(p => p.Infos).ThenInclude(i => i.Aceite)
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<List<PrpProposta>> ListRecebidasAsync(long empreendedorId, CancellationToken cancellationToken)
    {
        return _db.PrpPropostas
            .Include(p => p.Ideia)
            .Include(p => p.Usuario).ThenInclude(u => u!.Plano)
            .Include(p => p.Infos).ThenInclude(i => i.Aceite)
            .Where(p => p.Ideia != null && p.Ideia.UsuarioId == empreendedorId)
            .OrderByDescending(p => p.Usuario != null ? p.Usuario.PlanoId : 0)
            .ThenByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    // ← NOVO: lista propostas de uma ideia específica (para o empreendedor)
    public Task<List<PrpProposta>> ListByIdeiaAsync(long ideiaId, CancellationToken cancellationToken)
    {
        return _db.PrpPropostas
            .Include(p => p.Ideia)
            .Include(p => p.Usuario).ThenInclude(u => u!.Plano)
            .Include(p => p.Infos).ThenInclude(i => i.Aceite)
            .Where(p => p.IdeiaId == ideiaId && p.Status)
            .OrderByDescending(p => p.Usuario != null ? p.Usuario.PlanoId : 0)
            .ThenByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(PrpProposta proposta, CancellationToken cancellationToken) => _db.PrpPropostas.AddAsync(proposta, cancellationToken).AsTask();

    public Task<List<PrpProposta>> ListTodasAsync(CancellationToken cancellationToken)
    {
        return _db.PrpPropostas
            .Include(p => p.Infos)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPropostaAceitaEntreUsuariosAsync(long usuarioAId, long usuarioBId, long ideiaId, CancellationToken cancellationToken)
    {
        var propostas = await _db.PrpPropostas
            .Include(p => p.Ideia)
            .Include(p => p.Infos)
            .Where(p =>
                p.Status &&
                p.IdeiaId == ideiaId &&
                p.Ideia != null &&
                (
                    (p.UsuarioId == usuarioAId && p.Ideia.UsuarioId == usuarioBId) ||
                    (p.UsuarioId == usuarioBId && p.Ideia.UsuarioId == usuarioAId)
                ))
            .ToListAsync(cancellationToken);

        foreach (var p in propostas)
        {
            var ultima = p.Infos.OrderByDescending(i => i.CreateDate).FirstOrDefault();
            if (ultima?.AceiteId == 1) return true;
        }

        return false;
    }

    public void Update(PrpProposta proposta) => _db.PrpPropostas.Update(proposta);
}

public sealed class EfPagamentoRepository : IPagamentoRepository
{
    private readonly AppDbContext _db;

    public EfPagamentoRepository(AppDbContext db) => _db = db;

    public Task AddAsync(PgtPagamento pagamento, CancellationToken cancellationToken)
        => _db.PgtPagamentos.AddAsync(pagamento, cancellationToken).AsTask();

    public Task<List<PgtPagamento>> ListByUsuarioAsync(long usuarioId, CancellationToken cancellationToken)
        => _db.PgtPagamentos
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.CreateDate)
            .ToListAsync(cancellationToken);
}

public sealed class EfGovernancaRepository : IGovernancaRepository
{
    private readonly AppDbContext _db;

    public EfGovernancaRepository(AppDbContext db) => _db = db;

    public Task AddDenunciaAsync(GovDenuncia denuncia, CancellationToken cancellationToken)
        => _db.GovDenuncias.AddAsync(denuncia, cancellationToken).AsTask();

    public Task<List<GovDenuncia>> ListDenunciasAsync(CancellationToken cancellationToken)
        => _db.GovDenuncias.Include(d => d.Denunciante).OrderByDescending(d => d.CreateDate).ToListAsync(cancellationToken);

    public Task<GovDenuncia?> GetDenunciaByIdAsync(long id, CancellationToken cancellationToken)
        => _db.GovDenuncias.Include(d => d.Denunciante).FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public void UpdateDenuncia(GovDenuncia denuncia) => _db.GovDenuncias.Update(denuncia);
}

public sealed class NotificacaoRepository : INotificacaoRepository
{
    private readonly AppDbContext _db;

    public NotificacaoRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<NtfNotificacao?> GetByIdAsync(long ntfId, CancellationToken cancellationToken)
    {
        return _db.NtfNotificacoes
            .Include(n => n.Tipo)
            .FirstOrDefaultAsync(n => n.Id == ntfId, cancellationToken);
    }

    public Task<List<NtfNotificacao>> ListByUsuarioAsync(long usuarioId, CancellationToken cancellationToken)
    {
        return _db.NtfNotificacoes
            .Include(n => n.Tipo)
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.CreateDate)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(NtfNotificacao notificacao, CancellationToken cancellationToken) => _db.NtfNotificacoes.AddAsync(notificacao, cancellationToken).AsTask();

    public void Update(NtfNotificacao notificacao) => _db.NtfNotificacoes.Update(notificacao);
}

public sealed class LogRepository : ILogRepository
{
    private readonly AppDbContext _db;

    public LogRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(TrnLog log, CancellationToken cancellationToken) => _db.TrnLogs.AddAsync(log, cancellationToken).AsTask();
}

public sealed class LookupRepository : ILookupRepository
{
    private readonly AppDbContext _db;

    public LookupRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<IdaStatus?> GetIdeiaStatusByIdAsync(int statusId, CancellationToken cancellationToken)
        => _db.IdaStatuses.FirstOrDefaultAsync(s => s.Id == statusId, cancellationToken);

    public Task<IdaEstagio?> GetIdeiaEstagioByIdAsync(int estagioId, CancellationToken cancellationToken)
        => _db.IdaEstagios.FirstOrDefaultAsync(e => e.Id == estagioId, cancellationToken);

    public Task<IdaCategoria?> GetIdeiaCategoriaByIdAsync(int categoriaId, CancellationToken cancellationToken)
        => _db.IdaCategorias.FirstOrDefaultAsync(c => c.Id == categoriaId, cancellationToken);

    public Task<PrpAceite?> GetPropostaAceiteByIdAsync(int aceiteId, CancellationToken cancellationToken)
        => _db.PrpAceites.FirstOrDefaultAsync(a => a.Id == aceiteId, cancellationToken);

    public Task<NtfTipo?> GetNotificacaoTipoByIdAsync(int tipoId, CancellationToken cancellationToken)
        => _db.NtfTipos.FirstOrDefaultAsync(t => t.Id == tipoId, cancellationToken);

    public Task<TrnTipo?> GetLogTipoByIdAsync(int tipoId, CancellationToken cancellationToken)
        => _db.TrnTipos.FirstOrDefaultAsync(t => t.Id == tipoId, cancellationToken);
}
