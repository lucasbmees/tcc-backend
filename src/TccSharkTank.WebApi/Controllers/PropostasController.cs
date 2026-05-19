using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Application.Services;

namespace TccSharkTank.WebApi.Controllers;

[ApiController]
[Route("api")]
public sealed class PropostasController : ControllerBase
{
    private readonly IPropostaService _propostas;
    private readonly ICurrentUser _currentUser;

    public PropostasController(IPropostaService propostas, ICurrentUser currentUser)
    {
        _propostas = propostas;
        _currentUser = currentUser;
    }

    [Authorize(Roles = "investidor")]
    [HttpPost("ideias/{ideiaId:long}/propostas")]
    public Task<PropostaResponse> EnviarInicial([FromRoute] long ideiaId, [FromBody] CreatePropostaRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _propostas.EnviarInicialAsync(ideiaId, userId, request, cancellationToken);
    }

    [Authorize]
    [HttpGet("propostas/minhas")]
    public Task<List<PropostaResponse>> ListarMinhas(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _propostas.ListarMinhasAsync(userId, cancellationToken);
    }

    [Authorize(Roles = "empreendedor")]
    [HttpGet("propostas/recebidas")]
    public Task<List<PropostaResponse>> ListarRecebidas(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _propostas.ListarRecebidasAsync(userId, cancellationToken);
    }

    [Authorize(Roles = "empreendedor")]
    [HttpPost("propostas/{propostaId:long}/responder")]
    public Task<PropostaResponse> Responder([FromRoute] long propostaId, [FromBody] ResponderPropostaRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _propostas.ResponderAsync(propostaId, userId, request, cancellationToken);
    }

    [Authorize(Roles = "investidor")]
    [HttpPost("propostas/{propostaId:long}/responder-investidor")]
    public Task<PropostaResponse> ResponderInvestidor([FromRoute] long propostaId, [FromBody] ResponderPropostaRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _propostas.ResponderInvestidorAsync(propostaId, userId, request, cancellationToken);
    }

    [Authorize(Roles = "investidor")]
    [HttpPost("propostas/{propostaId:long}/encerrar")]
    public Task<PropostaResponse> Encerrar([FromRoute] long propostaId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _propostas.EncerrarAsync(propostaId, userId, cancellationToken);
    }

    [Authorize(Roles = "empreendedor")]
    [HttpGet("ideias/{ideiaId:long}/propostas")]
    public Task<List<PropostaResponse>> ListarDaIdeia(
        [FromRoute] long ideiaId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _propostas.ListarDaIdeiaAsync(ideiaId, userId, cancellationToken);
    }

    [Authorize]
    [HttpGet("propostas/{propostaId:long}/contrato")]
    public async Task<IActionResult> DownloadContrato([FromRoute] long propostaId, [FromServices] IJuridicoService juridico, CancellationToken cancellationToken)
    {
        var conteudo = await juridico.GerarTermoInvestimentoAsync(propostaId, cancellationToken);
        var bytes = System.Text.Encoding.UTF8.GetBytes(conteudo);
        return File(bytes, "text/markdown", $"Contrato-Investimento-{propostaId}.md");
    }
}
