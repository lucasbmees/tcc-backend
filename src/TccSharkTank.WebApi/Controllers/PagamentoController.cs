using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Application.Services;

namespace TccSharkTank.WebApi.Controllers;

[ApiController]
[Route("api/pagamentos")]
[Authorize]
public sealed class PagamentoController : ControllerBase
{
    private readonly IPagamentoService _pagamento;
    private readonly ICurrentUser _currentUser;

    public PagamentoController(IPagamentoService pagamento, ICurrentUser currentUser)
    {
        _pagamento = pagamento;
        _currentUser = currentUser;
    }

    [HttpPost("simular")]
    public Task<PagamentoResponse> Simular([FromBody] SimularPagamentoRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _pagamento.SimularPagamentoAsync(userId, request, cancellationToken);
    }

    [HttpGet("meus")]
    public Task<List<PagamentoResponse>> ListarMeus(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _pagamento.ListarMeusPagamentosAsync(userId, cancellationToken);
    }
}
