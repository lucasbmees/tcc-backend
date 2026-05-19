using TccSharkTank.Application.Abstractions.Persistence;
using TccSharkTank.Application.Abstractions.System;
using TccSharkTank.Application.Common;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Application.Services;

public interface ILogService
{
    Task RegistrarAsync(string tipoNome, long? usuarioId, long? ideiaId, long? propostaId, string descricao, CancellationToken cancellationToken);
}

public interface IIdeiaService
{
    Task<IdeiaDetailsResponse> CadastrarAsync(long usuarioId, CreateIdeiaRequest request, CancellationToken cancellationToken);
    Task<List<IdeiaDetailsResponse>> ListarAsync(
        string? termo,
        int? categoriaId,
        int? estagioId,
        string? regiao,
        decimal? valorMin,
        decimal? valorMax,
        CancellationToken cancellationToken);
    Task<IdeiaDetailsResponse> DetalhesAsync(long idaId, CancellationToken cancellationToken);
    Task<IdeiaDetailsResponse> EditarAsync(long idaId, long usuarioId, UpdateIdeiaRequest request, CancellationToken cancellationToken);
    Task<IdeiaDetailsResponse> AlterarStatusAsync(long idaId, ChangeIdeiaStatusRequest request, CancellationToken cancellationToken);
    Task<IdeiaDocumentoResponse> UploadDocumentoAsync(long idaId, long usuarioId, Stream pdfStream, string fileName, CancellationToken cancellationToken);
    Task<ComentarioResponse> ComentarAsync(long idaId, long usuarioId, CreateComentarioRequest request, CancellationToken cancellationToken);
}

public interface IPropostaService
{
    Task<PropostaResponse> EnviarInicialAsync(long ideiaId, long usuarioId, CreatePropostaRequest request, CancellationToken cancellationToken);
    Task<PropostaResponse> ResponderAsync(long propostaId, long usuarioId, ResponderPropostaRequest request, CancellationToken cancellationToken);
    Task<PropostaResponse> ResponderInvestidorAsync(long propostaId, long usuarioId, ResponderPropostaRequest request, CancellationToken cancellationToken);
    Task<List<PropostaResponse>> ListarMinhasAsync(long usuarioId, CancellationToken cancellationToken);
    Task<List<PropostaResponse>> ListarRecebidasAsync(long empreendedorId, CancellationToken cancellationToken);
    Task<PropostaResponse> EncerrarAsync(long propostaId, long usuarioId, CancellationToken cancellationToken);
    Task<List<PropostaResponse>> ListarDaIdeiaAsync(long ideiaId, long donoId, CancellationToken cancellationToken); // ← NOVO
}

public interface INotificacaoService
{
    Task<NotificacaoResponse> DispararAsync(DispararNotificacaoRequest request, CancellationToken cancellationToken);
    Task<List<NotificacaoResponse>> ListarMinhasAsync(long usuarioId, CancellationToken cancellationToken);
    Task<NotificacaoResponse> MarcarLidaAsync(long notificacaoId, long usuarioId, CancellationToken cancellationToken);
}

public sealed class LogService : ILogService
{
    private readonly ILogRepository _logs;
    private readonly ILookupRepository _lookup;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;

    public LogService(ILogRepository logs, ILookupRepository lookup, IUnitOfWork uow, IClock clock)
    {
        _logs = logs;
        _lookup = lookup;
        _uow = uow;
        _clock = clock;
    }

    public async Task RegistrarAsync(string tipoNome, long? usuarioId, long? ideiaId, long? propostaId, string descricao, CancellationToken cancellationToken)
    {
        var tipoId = tipoNome.Trim().ToLowerInvariant() switch
        {
            "cadastro" => 1,
            "edição" => 2,
            "edicao" => 2,
            "proposta" => 3,
            "login" => 4,
            _ => 0
        };

        if (tipoId == 0 || await _lookup.GetLogTipoByIdAsync(tipoId, cancellationToken) is null)
        {
            throw new AppException("Tipo de log inválido.", 400);
        }

        var log = new TrnLog
        {
            Id = 0,
            TipoId = tipoId,
            UsuarioId = usuarioId,
            IdeiaId = ideiaId,
            PropostaId = propostaId,
            CreateDate = _clock.UtcNow,
            Descricao = descricao
        };

        await _logs.AddAsync(log, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

public sealed class IdeiaService : IIdeiaService
{
    private readonly IIdeiaRepository _ideias;
    private readonly ILookupRepository _lookup;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IFileStorage _fileStorage;
    private readonly ILogService _logs;
    private readonly INotificacaoService _notificacoes;

    public IdeiaService(
        IIdeiaRepository ideias, 
        ILookupRepository lookup, 
        IUnitOfWork uow, 
        IClock clock, 
        IFileStorage fileStorage, 
        ILogService logs,
        INotificacaoService notificacoes)
    {
        _ideias = ideias;
        _lookup = lookup;
        _uow = uow;
        _clock = clock;
        _fileStorage = fileStorage;
        _logs = logs;
        _notificacoes = notificacoes;
    }

    public async Task<IdeiaDetailsResponse> CadastrarAsync(long usuarioId, CreateIdeiaRequest request, CancellationToken cancellationToken)
    {
        if (await _lookup.GetIdeiaCategoriaByIdAsync(request.CategoriaId, cancellationToken) is null)
        {
            throw new AppException("Categoria inválida.", 400);
        }

        if (await _lookup.GetIdeiaEstagioByIdAsync(request.EstagioId, cancellationToken) is null)
        {
            throw new AppException("Estágio inválido.", 400);
        }

        var ideia = new IdaIdeia
        {
            Id = 0,
            UsuarioId = usuarioId,
            StatusId = 1,
            MotivoStatus = null,
            CategoriaId = request.CategoriaId,
            EstagioId = request.EstagioId,
            Nome = request.Nome.Trim(),
            Regiao = request.Regiao?.Trim(),
            CreateDate = _clock.UtcNow,
            UpdateDate = _clock.UtcNow
        };

        ideia.Info = new IdaInfo
        {
            Id = 0,
            Cnpj = request.Cnpj.Trim(),
            Descricao = request.Descricao,
            LinkVideo = request.LinkVideo,
            Imagem = request.Imagem,
            Fatia = request.Fatia,
            ValorCaptacao = request.ValorCaptacao,
            CreateDate = _clock.UtcNow,
            UpdateDate = _clock.UtcNow
        };

        await _ideias.AddAsync(ideia, cancellationToken);
        await _logs.RegistrarAsync("cadastro", usuarioId, ideiaId: null, propostaId: null, descricao: $"Cadastro de ideia {ideia.Nome}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return await DetalhesAsync(ideia.Id, cancellationToken);
    }

    public async Task<List<IdeiaDetailsResponse>> ListarAsync(
        string? termo,
        int? categoriaId,
        int? estagioId,
        string? regiao,
        decimal? valorMin,
        decimal? valorMax,
        CancellationToken cancellationToken)
    {
        var ideias = await _ideias.ListAsync(termo, categoriaId, estagioId, regiao, valorMin, valorMax, cancellationToken);
        return ideias.Select(MapIdeia).ToList();
    }

    public async Task<IdeiaDetailsResponse> DetalhesAsync(long idaId, CancellationToken cancellationToken)
    {
        var ideia = await _ideias.GetByIdAsync(idaId, cancellationToken);
        if (ideia is null)
        {
            throw new AppException("Ideia não encontrada.", 404);
        }

        return MapIdeia(ideia);
    }

    public async Task<IdeiaDetailsResponse> EditarAsync(long idaId, long usuarioId, UpdateIdeiaRequest request, CancellationToken cancellationToken)
    {
        var ideia = await _ideias.GetByIdAsync(idaId, cancellationToken);
        if (ideia is null)
        {
            throw new AppException("Ideia não encontrada.", 404);
        }

        if (ideia.UsuarioId != usuarioId)
        {
            throw new AppException("Você não tem permissão para editar esta ideia.", 403);
        }

        if (request.CategoriaId.HasValue)
        {
            if (await _lookup.GetIdeiaCategoriaByIdAsync(request.CategoriaId.Value, cancellationToken) is null)
            {
                throw new AppException("Categoria inválida.", 400);
            }

            ideia.CategoriaId = request.CategoriaId.Value;
        }

        if (request.EstagioId.HasValue)
        {
            if (await _lookup.GetIdeiaEstagioByIdAsync(request.EstagioId.Value, cancellationToken) is null)
            {
                throw new AppException("Estágio inválido.", 400);
            }

            ideia.EstagioId = request.EstagioId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Nome))
        {
            ideia.Nome = request.Nome.Trim();
        }

        if (request.Regiao is not null)
        {
            ideia.Regiao = request.Regiao.Trim();
        }

        ideia.UpdateDate = _clock.UtcNow;

        ideia.Info ??= new IdaInfo
        {
            Id = 0,
            Cnpj = request.Cnpj?.Trim() ?? "",
            CreateDate = _clock.UtcNow,
            UpdateDate = _clock.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(request.Cnpj))
        {
            ideia.Info.Cnpj = request.Cnpj.Trim();
        }

        if (request.Descricao is not null)
        {
            ideia.Info.Descricao = request.Descricao;
        }
        if (request.LinkVideo is not null)
        {
            ideia.Info.LinkVideo = request.LinkVideo;
        }
        if (request.Imagem is not null)
        {
            ideia.Info.Imagem = request.Imagem;
        }
        if (request.Fatia.HasValue)
        {
            ideia.Info.Fatia = request.Fatia.Value;
        }
        if (request.ValorCaptacao.HasValue)
        {
            ideia.Info.ValorCaptacao = request.ValorCaptacao.Value;
        }

        ideia.Info.UpdateDate = _clock.UtcNow;

        _ideias.Update(ideia);
        await _logs.RegistrarAsync("edição", usuarioId, ideiaId: ideia.Id, propostaId: null, descricao: $"Edição de ideia {ideia.Nome}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return await DetalhesAsync(idaId, cancellationToken);
    }

    public async Task<IdeiaDetailsResponse> AlterarStatusAsync(long idaId, ChangeIdeiaStatusRequest request, CancellationToken cancellationToken)
    {
        var ideia = await _ideias.GetByIdAsync(idaId, cancellationToken);
        if (ideia is null)
        {
            throw new AppException("Ideia não encontrada.", 404);
        }

        if (await _lookup.GetIdeiaStatusByIdAsync(request.StatusId, cancellationToken) is null)
        {
            throw new AppException("Status inválido.", 400);
        }

        ideia.StatusId = request.StatusId;
        ideia.MotivoStatus = request.Motivo;

        _ideias.Update(ideia);
        await _logs.RegistrarAsync("edição", usuarioId: null, ideiaId: ideia.Id, propostaId: null, descricao: $"Alteração de status da ideia {ideia.Nome} para {request.StatusId}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return await DetalhesAsync(idaId, cancellationToken);
    }

    public async Task<IdeiaDocumentoResponse> UploadDocumentoAsync(long idaId, long usuarioId, Stream pdfStream, string fileName, CancellationToken cancellationToken)
    {
        var ideia = await _ideias.GetByIdAsync(idaId, cancellationToken);
        if (ideia is null)
        {
            throw new AppException("Ideia não encontrada.", 404);
        }

        if (ideia.UsuarioId != usuarioId)
        {
            throw new AppException("Você não tem permissão para enviar documento nesta ideia.", 403);
        }

        var storedPath = await _fileStorage.SavePdfAsync(pdfStream, fileName, cancellationToken);

        var doc = new IdaDocumento
        {
            IdeiaId = idaId,
            Arquivo = storedPath
        };

        ideia.Documentos.Add(doc);
        _ideias.Update(ideia);
        await _logs.RegistrarAsync("edição", usuarioId: null, ideiaId: ideia.Id, propostaId: null, descricao: $"Upload de documento para ideia {ideia.Nome}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new IdeiaDocumentoResponse(doc.Id, doc.Arquivo);
    }

    public async Task<ComentarioResponse> ComentarAsync(long idaId, long usuarioId, CreateComentarioRequest request, CancellationToken cancellationToken)
    {
        var ideia = await _ideias.GetByIdAsync(idaId, cancellationToken);
        if (ideia is null) throw new AppException("Ideia não encontrada.", 404);

        var comentario = new IdaComentario
        {
            Id = 0,
            IdeiaId = idaId,
            UsuarioId = usuarioId,
            ParentId = request.ParentId,
            Texto = request.Texto.Trim(),
            CreateDate = _clock.UtcNow,
            UpdateDate = _clock.UtcNow
        };

        await _ideias.AddComentarioAsync(comentario, cancellationToken);
        await _logs.RegistrarAsync("comentário", usuarioId, ideiaId: idaId, propostaId: null, descricao: $"Novo comentário na ideia {ideia.Nome}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // Notificar o dono da ideia se não for ele mesmo comentando
        if (ideia.UsuarioId != usuarioId)
        {
            await _notificacoes.DispararAsync(new DispararNotificacaoRequest(
                UsuarioId: ideia.UsuarioId,
                TipoId: 3,
                Mensagem: $"Novo comentário na sua ideia '{ideia.Nome}'"
            ), cancellationToken);
        }

        return MapComentario(comentario);
    }

    private static IdeiaDetailsResponse MapIdeia(IdaIdeia i)
    {
        return new IdeiaDetailsResponse(
            IdaId: i.Id,
            IdaUsuarioId: i.UsuarioId,
            IdaNome: i.Nome,
            Regiao: i.Regiao,
            IdaCategoriaId: i.CategoriaId,
            CategoriaNome: i.Categoria?.Nome ?? i.CategoriaId.ToString(),
            IdaEstagioId: i.EstagioId,
            EstagioNome: i.Estagio?.Nome ?? i.EstagioId.ToString(),
            IdaStatusId: i.StatusId,
            StatusNome: i.Status?.Nome ?? i.StatusId.ToString(),
            IdaMotivoStatus: i.MotivoStatus,
            Info: i.Info is null
                ? null
                : new IdeiaInfoResponse(
                    IdaInfoCnpj: i.Info.Cnpj,
                    IdaInfoDescricao: i.Info.Descricao,
                    IdaInfoLinkVideo: i.Info.LinkVideo,
                    IdaInfoImagem: i.Info.Imagem,
                    IdaInfoFatia: i.Info.Fatia,
                    IdaInfoValorCaptacao: i.Info.ValorCaptacao,
                    CreateDate: i.Info.CreateDate,
                    UpdateDate: i.Info.UpdateDate),
            Documentos: i.Documentos.Select(d => new IdeiaDocumentoResponse(d.Id, d.Arquivo)).ToList(),
            Comentarios: i.Comentarios.Where(c => c.ParentId == null).Select(MapComentario).ToList()
        );
    }

    private static ComentarioResponse MapComentario(IdaComentario c)
    {
        return new ComentarioResponse(
            Id: c.Id,
            UsuarioId: c.UsuarioId,
            UsuarioNome: c.Usuario != null ? $"{c.Usuario.Nome} {c.Usuario.Sobrenome}" : "Usuário",
            ParentId: c.ParentId,
            Texto: c.Texto,
            CreateDate: c.CreateDate,
            Replies: c.Replies.Select(MapComentario).ToList()
        );
    }
}

public sealed class PropostaService : IPropostaService
{
    private readonly IPropostaRepository _propostas;
    private readonly IIdeiaRepository _ideias;
    private readonly INotificacaoRepository _notificacoes;
    private readonly ILookupRepository _lookup;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly ILogService _logs;

    private const int NtfTipoPrpAceita = 1;
    private const int NtfTipoPrpRecusada = 2;
    private const int NtfTipoAlerta = 3;
    private const int NtfTipoPrpRecebida = 5;
    private const int NtfTipoPrpContraproposta = 6;

    public PropostaService(
        IPropostaRepository propostas,
        IIdeiaRepository ideias,
        INotificacaoRepository notificacoes,
        ILookupRepository lookup,
        IUnitOfWork uow,
        IClock clock,
        ILogService logs)
    {
        _propostas = propostas;
        _ideias = ideias;
        _notificacoes = notificacoes;
        _lookup = lookup;
        _uow = uow;
        _clock = clock;
        _logs = logs;
    }

    public async Task<PropostaResponse> EnviarInicialAsync(long ideiaId, long usuarioId, CreatePropostaRequest request, CancellationToken cancellationToken)
    {
        var ideia = await _ideias.GetByIdAsync(ideiaId, cancellationToken);
        if (ideia is null)
        {
            throw new AppException("Ideia não encontrada.", 404);
        }

        var aceitePendente = 3;
        if (await _lookup.GetPropostaAceiteByIdAsync(aceitePendente, cancellationToken) is null)
        {
            throw new AppException("Configuração de aceite inválida.", 500);
        }

        var proposta = new PrpProposta
        {
            Id = 0,
            IdeiaId = ideiaId,
            UsuarioId = usuarioId,
            Status = true,
            CreateDate = _clock.UtcNow,
            UpdateDate = _clock.UtcNow
        };

        proposta.Infos.Add(new PrpInfo
        {
            Id = 0,
            Mensagem = request.Mensagem,
            Valor = request.Valor,
            FatiaPret = request.FatiaPret,
            AceiteId = aceitePendente,
            Retorno = null,
            CreateDate = _clock.UtcNow,
            UpdateDate = _clock.UtcNow
        });

        await _propostas.AddAsync(proposta, cancellationToken);
        if (await _lookup.GetNotificacaoTipoByIdAsync(NtfTipoPrpRecebida, cancellationToken) is null)
        {
            throw new AppException("Configuração de notificação (prp recebida) inválida.", 500);
        }

        await _notificacoes.AddAsync(new NtfNotificacao
        {
            Id = 0,
            UsuarioId = ideia.UsuarioId,
            TipoId = NtfTipoPrpRecebida,
            Mensagem = $"Você recebeu uma proposta na ideia '{ideia.Nome}'",
            Lida = false,
            CreateDate = _clock.UtcNow
        }, cancellationToken);
        await _logs.RegistrarAsync("proposta", usuarioId: usuarioId, ideiaId: ideiaId, propostaId: null, descricao: $"Envio de proposta para ideia {ideia.Nome}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return await MapAsync(proposta.Id, cancellationToken);
    }

    public async Task<PropostaResponse> ResponderAsync(long propostaId, long usuarioId, ResponderPropostaRequest request, CancellationToken cancellationToken)
    {
        var proposta = await _propostas.GetByIdAsync(propostaId, cancellationToken);
        if (proposta is null)
        {
            throw new AppException("Proposta não encontrada.", 404);
        }

        var ideia = proposta.Ideia ?? await _ideias.GetByIdAsync(proposta.IdeiaId, cancellationToken);
        if (ideia is null)
        {
            throw new AppException("Ideia não encontrada.", 404);
        }

        if (ideia.UsuarioId != usuarioId)
        {
            throw new AppException("Você não tem permissão para responder esta proposta.", 403);
        }

        if (await _lookup.GetPropostaAceiteByIdAsync(request.AceiteId, cancellationToken) is null)
        {
            throw new AppException("Aceite inválido.", 400);
        }

        if (request.AceiteId == 4 && string.IsNullOrWhiteSpace(request.Retorno))
        {
            throw new AppException("O retorno é obrigatório para contraproposta.", 400);
        }

        proposta.Infos.Add(new PrpInfo
        {
            Id = 0,
            Mensagem = null,
            Valor = 0,
            FatiaPret = 0,
            AceiteId = request.AceiteId,
            Retorno = request.Retorno,
            CreateDate = _clock.UtcNow,
            UpdateDate = _clock.UtcNow
        });

        _propostas.Update(proposta);

        var notifTipoId = request.AceiteId switch
        {
            1 => NtfTipoPrpAceita,
            2 => NtfTipoPrpRecusada,
            4 => NtfTipoPrpContraproposta,
            _ => NtfTipoAlerta
        };

        if (await _lookup.GetNotificacaoTipoByIdAsync(notifTipoId, cancellationToken) is null)
        {
            throw new AppException("Configuração de notificação inválida.", 500);
        }

        var mensagem = request.AceiteId switch
        {
            1 => $"Sua proposta para a ideia #{ideia.Id} foi aceita.",
            2 => $"Sua proposta para a ideia #{ideia.Id} foi recusada.",
            4 => $"O empreendedor enviou uma contraproposta na ideia #{ideia.Id}: \"{request.Retorno!.Trim()}\"",
            _ => $"A proposta da ideia #{ideia.Id} teve uma atualização."
        };

        await _notificacoes.AddAsync(new NtfNotificacao
        {
            Id = 0,
            UsuarioId = proposta.UsuarioId,
            TipoId = notifTipoId,
            Mensagem = mensagem,
            Lida = false,
            CreateDate = _clock.UtcNow
        }, cancellationToken);

        await _logs.RegistrarAsync("proposta", usuarioId: usuarioId, ideiaId: ideia.Id, propostaId: proposta.Id, descricao: $"Resposta de proposta {proposta.Id} para ideia {ideia.Nome}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return await MapAsync(proposta.Id, cancellationToken);
    }

    public async Task<PropostaResponse> ResponderInvestidorAsync(long propostaId, long usuarioId, ResponderPropostaRequest request, CancellationToken cancellationToken)
    {
        var proposta = await _propostas.GetByIdAsync(propostaId, cancellationToken);
        if (proposta is null)
        {
            throw new AppException("Proposta não encontrada.", 404);
        }

        if (proposta.UsuarioId != usuarioId)
        {
            throw new AppException("Você não tem permissão para responder esta proposta.", 403);
        }

        if (request.AceiteId is not (1 or 2))
        {
            throw new AppException("Aceite inválido.", 400);
        }

        if (await _lookup.GetPropostaAceiteByIdAsync(request.AceiteId, cancellationToken) is null)
        {
            throw new AppException("Aceite inválido.", 400);
        }

        var ultima = proposta.Infos.OrderByDescending(i => i.CreateDate).FirstOrDefault();
        if (ultima is null || ultima.AceiteId != 4 || string.IsNullOrWhiteSpace(ultima.Retorno))
        {
            throw new AppException("Não existe contraproposta pendente para responder.", 400);
        }

        proposta.Infos.Add(new PrpInfo
        {
            Id = 0,
            Mensagem = null,
            Valor = 0,
            FatiaPret = 0,
            AceiteId = request.AceiteId,
            Retorno = null,
            CreateDate = _clock.UtcNow,
            UpdateDate = _clock.UtcNow
        });

        _propostas.Update(proposta);

        var ideia = proposta.Ideia ?? await _ideias.GetByIdAsync(proposta.IdeiaId, cancellationToken);
        if (ideia is null)
        {
            throw new AppException("Ideia não encontrada.", 404);
        }

        if (await _lookup.GetNotificacaoTipoByIdAsync(NtfTipoAlerta, cancellationToken) is null)
        {
            throw new AppException("Configuração de notificação inválida.", 500);
        }

        var mensagem = request.AceiteId == 1
            ? $"O investidor aceitou a contraproposta na ideia #{ideia.Id}."
            : $"O investidor recusou a contraproposta na ideia #{ideia.Id}.";

        await _notificacoes.AddAsync(new NtfNotificacao
        {
            UsuarioId = ideia.UsuarioId,
            TipoId = NtfTipoAlerta,
            Mensagem = mensagem,
            Lida = false,
            CreateDate = _clock.UtcNow
        }, cancellationToken);

        await _logs.RegistrarAsync("proposta", usuarioId: usuarioId, ideiaId: ideia.Id, propostaId: proposta.Id, descricao: $"Resposta do investidor para contraproposta {proposta.Id}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return await MapAsync(proposta.Id, cancellationToken);
    }

    public async Task<List<PropostaResponse>> ListarMinhasAsync(long usuarioId, CancellationToken cancellationToken)
    {
        var propostas = await _propostas.ListByUsuarioAsync(usuarioId, cancellationToken);
        return propostas.Select(Map).ToList();
    }

    public async Task<List<PropostaResponse>> ListarRecebidasAsync(long empreendedorId, CancellationToken cancellationToken)
    {
        var propostas = await _propostas.ListRecebidasAsync(empreendedorId, cancellationToken);
        return propostas.Select(Map).ToList();
    }

    public async Task<PropostaResponse> EncerrarAsync(long propostaId, long usuarioId, CancellationToken cancellationToken)
    {
        var proposta = await _propostas.GetByIdAsync(propostaId, cancellationToken);
        if (proposta is null)
        {
            throw new AppException("Proposta não encontrada.", 404);
        }

        if (proposta.UsuarioId != usuarioId)
        {
            throw new AppException("Você não tem permissão para encerrar esta proposta.", 403);
        }

        proposta.Status = false;
        _propostas.Update(proposta);
        await _logs.RegistrarAsync("proposta", usuarioId: usuarioId, ideiaId: proposta.IdeiaId, propostaId: proposta.Id, descricao: $"Encerramento da proposta {proposta.Id}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return Map(proposta);
    }

    // ── NOVO: lista propostas recebidas em uma ideia do empreendedor ──────────
    public async Task<List<PropostaResponse>> ListarDaIdeiaAsync(long ideiaId, long donoId, CancellationToken cancellationToken)
    {
        var ideia = await _ideias.GetByIdAsync(ideiaId, cancellationToken)
            ?? throw new AppException("Ideia não encontrada.", 404);

        if (ideia.UsuarioId != donoId)
            throw new AppException("Sem permissão para ver as propostas desta ideia.", 403);

        var propostas = await _propostas.ListByIdeiaAsync(ideiaId, cancellationToken);
        return propostas.Select(Map).ToList();
    }
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<PropostaResponse> MapAsync(long prpId, CancellationToken cancellationToken)
    {
        var proposta = await _propostas.GetByIdAsync(prpId, cancellationToken);
        if (proposta is null)
        {
            throw new AppException("Proposta não encontrada.", 404);
        }
        return Map(proposta);
    }

    private static PropostaResponse Map(PrpProposta p)
    {
        return new PropostaResponse(
            PrpId: p.Id,
            PrpIdeiaId: p.IdeiaId,
            PrpUsuarioId: p.UsuarioId,
            PrpStatus: p.Status,
            Infos: p.Infos
                .OrderBy(i => i.CreateDate)
                .Select(i => new PropostaInfoResponse(
                    Mensagem: i.Mensagem,
                    Valor: i.Valor,
                    FatiaPret: i.FatiaPret,
                    AceiteId: i.AceiteId,
                    AceiteNome: i.Aceite?.Nome ?? i.AceiteId.ToString(),
                    Retorno: i.Retorno,
                    CreateDate: i.CreateDate,
                    UpdateDate: i.UpdateDate))
                .ToList()
        );
    }
}

public sealed class NotificacaoService : INotificacaoService
{
    private readonly INotificacaoRepository _notificacoes;
    private readonly ILookupRepository _lookup;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IEmailService _email;
    private readonly IUsuarioRepository _usuarios;

    public NotificacaoService(
        INotificacaoRepository notificacoes, 
        ILookupRepository lookup, 
        IUnitOfWork uow, 
        IClock clock,
        IEmailService email,
        IUsuarioRepository usuarios)
    {
        _notificacoes = notificacoes;
        _lookup = lookup;
        _uow = uow;
        _clock = clock;
        _email = email;
        _usuarios = usuarios;
    }

    public async Task<NotificacaoResponse> DispararAsync(DispararNotificacaoRequest request, CancellationToken cancellationToken)
    {
        var tipo = await _lookup.GetNotificacaoTipoByIdAsync(request.TipoId, cancellationToken);
        if (tipo is null)
        {
            throw new AppException("Tipo de notificação inválido.", 400);
        }

        var notif = new NtfNotificacao
        {
            Id = 0,
            UsuarioId = request.UsuarioId,
            TipoId = request.TipoId,
            Mensagem = request.Mensagem,
            Lida = false,
            CreateDate = _clock.UtcNow
        };

        await _notificacoes.AddAsync(notif, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // Envio de E-mail Externo (Módulo 7)
        var usuario = await _usuarios.GetByIdAsync(request.UsuarioId, cancellationToken);
        if (usuario?.Perfil != null)
        {
            bool deveEnviar = request.TipoId switch
            {
                1 or 2 or 5 or 6 => usuario.Perfil.ReceberEmailPropostas, // Tipos de proposta
                3 => usuario.Perfil.ReceberEmailAlertas,                 // Alertas
                _ => usuario.Perfil.ReceberEmailMensagens                // Chat e outros
            };

            if (deveEnviar)
            {
                await _email.EnviarAsync(
                    para: usuario.Email,
                    assunto: $"[TCC Shark Tank] Nova Notificação: {tipo.Nome}",
                    corpo: $"Olá {usuario.Nome},\n\nVocê recebeu uma nova notificação em nossa plataforma:\n\n\"{request.Mensagem}\"\n\nAcesse agora para conferir!",
                    cancellationToken: cancellationToken
                );
            }
        }

        return new NotificacaoResponse(notif.Id, notif.TipoId, tipo.Nome, notif.Mensagem, notif.Lida, notif.CreateDate);
    }

    public async Task<List<NotificacaoResponse>> ListarMinhasAsync(long usuarioId, CancellationToken cancellationToken)
    {
        var items = await _notificacoes.ListByUsuarioAsync(usuarioId, cancellationToken);
        return items
            .OrderByDescending(n => n.CreateDate)
            .Select(n => new NotificacaoResponse(n.Id, n.TipoId, n.Tipo?.Nome ?? n.TipoId.ToString(), n.Mensagem, n.Lida, n.CreateDate))
            .ToList();
    }

    public async Task<NotificacaoResponse> MarcarLidaAsync(long notificacaoId, long usuarioId, CancellationToken cancellationToken)
    {
        var n = await _notificacoes.GetByIdAsync(notificacaoId, cancellationToken);
        if (n is null)
        {
            throw new AppException("Notificação não encontrada.", 404);
        }

        if (n.UsuarioId != usuarioId)
        {
            throw new AppException("Você não tem permissão para alterar esta notificação.", 403);
        }

        n.Lida = true;
        _notificacoes.Update(n);
        await _uow.SaveChangesAsync(cancellationToken);
        return new NotificacaoResponse(n.Id, n.TipoId, n.Tipo?.Nome ?? n.TipoId.ToString(), n.Mensagem, n.Lida, n.CreateDate);
    }
}
