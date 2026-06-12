using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Application.Services;

namespace TccSharkTank.WebApi.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public sealed class ChatController : ControllerBase
{
    private readonly IChatService _chat;
    private readonly ICurrentUser _currentUser;

    public ChatController(IChatService chat, ICurrentUser currentUser)
    {
        _chat = chat;
        _currentUser = currentUser;
    }

    [HttpGet("conversas")]
    public Task<List<ConversaResponse>> ListMinhasConversas(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _chat.ListarMinhasConversasAsync(userId, cancellationToken);
    }

    [HttpGet("conversas/{conversaId:long}/mensagens")]
    public Task<List<MensagemResponse>> ListMensagens([FromRoute] long conversaId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _chat.ListarMensagensAsync(conversaId, userId, cancellationToken);
    }

    [HttpPost("mensagens")]
    public Task<MensagemResponse> EnviarNova([FromBody] CreateMensagemRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _chat.EnviarMensagemAsync(userId, null, request, cancellationToken);
    }

    [HttpPost("conversas/{conversaId:long}/mensagens")]
    public Task<MensagemResponse> EnviarNaConversa([FromRoute] long conversaId, [FromBody] CreateMensagemRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _chat.EnviarMensagemAsync(userId, conversaId, request, cancellationToken);
    }
}
