using TccSharkTank.Application.Abstractions.Persistence;
using TccSharkTank.Application.Abstractions.System;
using TccSharkTank.Application.Common;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Application.Services;

public interface IChatService
{
    Task<List<ConversaResponse>> ListarMinhasConversasAsync(long usuarioId, CancellationToken cancellationToken);
    Task<List<MensagemResponse>> ListarMensagensAsync(long conversaId, long usuarioId, CancellationToken cancellationToken);
    Task<MensagemResponse> EnviarMensagemAsync(long usuarioId, long? conversaId, CreateMensagemRequest request, CancellationToken cancellationToken);
}

public sealed class ChatService : IChatService
{
    private readonly IChatRepository _chat;
    private readonly IUsuarioRepository _usuarios;
    private readonly IPropostaRepository _propostas;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly INotificacaoService _notificacoes;

    public ChatService(
        IChatRepository chat,
        IUsuarioRepository usuarios,
        IPropostaRepository propostas,
        IUnitOfWork uow,
        IClock clock,
        INotificacaoService notificacoes)
    {
        _chat = chat;
        _usuarios = usuarios;
        _propostas = propostas;
        _uow = uow;
        _clock = clock;
        _notificacoes = notificacoes;
    }

    public async Task<List<ConversaResponse>> ListarMinhasConversasAsync(long usuarioId, CancellationToken cancellationToken)
    {
        var conversas = await _chat.ListMinhasConversasAsync(usuarioId, cancellationToken);
        var permitidas = new List<ChtConversa>();

        foreach (var c in conversas)
        {
            if (!c.IdeiaId.HasValue) continue;
            var outroId = c.Usuario1Id == usuarioId ? c.Usuario2Id : c.Usuario1Id;
            if (await _propostas.HasPropostaAceitaEntreUsuariosAsync(usuarioId, outroId, c.IdeiaId.Value, cancellationToken))
            {
                permitidas.Add(c);
            }
        }

        return permitidas.Select(c => MapConversa(c, usuarioId)).ToList();
    }

    public async Task<List<MensagemResponse>> ListarMensagensAsync(long conversaId, long usuarioId, CancellationToken cancellationToken)
    {
        var conversa = await _chat.GetConversaByIdAsync(conversaId, cancellationToken);
        if (conversa is null || (conversa.Usuario1Id != usuarioId && conversa.Usuario2Id != usuarioId))
        {
            throw new AppException("Conversa não encontrada.", 404);
        }

        if (!conversa.IdeiaId.HasValue)
        {
            throw new AppException("Conversa só é permitida após uma proposta aceita.", 403);
        }

        var outroId = conversa.Usuario1Id == usuarioId ? conversa.Usuario2Id : conversa.Usuario1Id;
        if (!await _propostas.HasPropostaAceitaEntreUsuariosAsync(usuarioId, outroId, conversa.IdeiaId.Value, cancellationToken))
        {
            throw new AppException("Conversa só é permitida após uma proposta aceita.", 403);
        }

        var mensagens = await _chat.GetMensagensByConversaIdAsync(conversaId, cancellationToken);

        // Marcar como lidas mensagens do outro usuário
        var mensagensNaoLidas = mensagens.Where(m => m.RemetenteId != usuarioId && !m.Lida).ToList();
        if (mensagensNaoLidas.Any())
        {
            foreach (var m in mensagensNaoLidas) m.Lida = true;
            await _uow.SaveChangesAsync(cancellationToken);
        }

        return mensagens.Select(MapMensagem).ToList();
    }

    public async Task<MensagemResponse> EnviarMensagemAsync(long usuarioId, long? conversaId, CreateMensagemRequest request, CancellationToken cancellationToken)
    {
        ChtConversa? conversa;
        long outroUsuarioId;
        long ideiaId;

        if (conversaId.HasValue)
        {
            conversa = await _chat.GetConversaByIdAsync(conversaId.Value, cancellationToken);
            if (conversa is null || (conversa.Usuario1Id != usuarioId && conversa.Usuario2Id != usuarioId))
            {
                throw new AppException("Conversa não encontrada.", 404);
            }

            if (!conversa.IdeiaId.HasValue)
            {
                throw new AppException("Conversa só é permitida após uma proposta aceita.", 403);
            }

            outroUsuarioId = conversa.Usuario1Id == usuarioId ? conversa.Usuario2Id : conversa.Usuario1Id;
            ideiaId = conversa.IdeiaId.Value;
        }
        else if (request.ParaUsuarioId.HasValue)
        {
            if (usuarioId == request.ParaUsuarioId.Value)
                throw new AppException("Você não pode conversar consigo mesmo.", 400);

            if (!request.IdeiaId.HasValue)
            {
                throw new AppException("Conversa só é permitida após uma proposta aceita.", 403);
            }

            outroUsuarioId = request.ParaUsuarioId.Value;
            ideiaId = request.IdeiaId.Value;

            if (!await _propostas.HasPropostaAceitaEntreUsuariosAsync(usuarioId, outroUsuarioId, ideiaId, cancellationToken))
            {
                throw new AppException("Conversa só é permitida após uma proposta aceita.", 403);
            }

            // Tenta achar conversa existente
            conversa = await _chat.GetConversaEntreUsuariosAsync(usuarioId, request.ParaUsuarioId.Value, request.IdeiaId, cancellationToken);

            if (conversa is null)
            {
                conversa = new ChtConversa
                {
                    Id = 0,
                    Usuario1Id = usuarioId,
                    Usuario2Id = request.ParaUsuarioId.Value,
                    IdeiaId = request.IdeiaId,
                    CreateDate = _clock.UtcNow,
                    UpdateDate = _clock.UtcNow
                };
                await _chat.AddConversaAsync(conversa, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            throw new AppException("Informe a conversa ou o destinatário.", 400);
        }

        if (!await _propostas.HasPropostaAceitaEntreUsuariosAsync(usuarioId, outroUsuarioId, ideiaId, cancellationToken))
        {
            throw new AppException("Conversa só é permitida após uma proposta aceita.", 403);
        }

        var mensagem = new ChtMensagem
        {
            Id = 0,
            ConversaId = conversa.Id,
            RemetenteId = usuarioId,
            Texto = request.Texto.Trim(),
            Lida = false,
            CreateDate = _clock.UtcNow,
            UpdateDate = _clock.UtcNow
        };

        conversa.UpdateDate = _clock.UtcNow;
        await _chat.AddMensagemAsync(mensagem, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // Notificar destinatário
        long destinatarioId = conversa.Usuario1Id == usuarioId ? conversa.Usuario2Id : conversa.Usuario1Id;
        await _notificacoes.DispararAsync(new DispararNotificacaoRequest(
            UsuarioId: destinatarioId,
            TipoId: 5, // Chat
            Mensagem: "Você recebeu uma nova mensagem privada."
        ), cancellationToken);

        return MapMensagem(mensagem);
    }

    private static ConversaResponse MapConversa(ChtConversa c, long usuarioId)
    {
        var outroUsuario = c.Usuario1Id == usuarioId ? c.Usuario2 : c.Usuario1;
        return new ConversaResponse(
            Id: c.Id,
            OutroUsuarioId: outroUsuario?.Id ?? 0,
            OutroUsuarioNome: outroUsuario != null ? $"{outroUsuario.Nome} {outroUsuario.Sobrenome}" : "Usuário",
            IdeiaId: c.IdeiaId,
            IdeiaNome: c.Ideia?.Nome,
            UpdateDate: c.UpdateDate,
            UltimaMensagem: c.Mensagens.OrderByDescending(m => m.CreateDate).Select(MapMensagem).FirstOrDefault()
        );
    }

    private static MensagemResponse MapMensagem(ChtMensagem m)
    {
        return new MensagemResponse(
            Id: m.Id,
            RemetenteId: m.RemetenteId,
            RemetenteNome: m.Remetente != null ? $"{m.Remetente.Nome} {m.Remetente.Sobrenome}" : "Usuário",
            Texto: m.Texto,
            Lida: m.Lida,
            CreateDate: m.CreateDate
        );
    }
}
