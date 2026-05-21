using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Application.Services;

namespace TccSharkTank.WebApi.Controllers;

[ApiController]
[Route("api/planos")]
[Authorize]
public sealed class PlanosController : ControllerBase
{
    private readonly IPlanoService _planos;
    private readonly ICurrentUser _currentUser;

    public PlanosController(IPlanoService planos, ICurrentUser currentUser)
    {
        _planos = planos;
        _currentUser = currentUser;
    }

    [HttpGet]
    public Task<List<PlanoInfoResponse>> Listar(CancellationToken cancellationToken)
        => _planos.ListarAsync(cancellationToken);

    [HttpGet("meu")]
    public Task<PlanoMeuResponse> Meu(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _planos.MeuAsync(userId, cancellationToken);
    }

    [HttpPost("assinar")]
    public Task<AssinarPlanoResponse> Assinar([FromBody] AssinarPlanoRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _planos.AssinarAsync(userId, request, cancellationToken);
    }
}

