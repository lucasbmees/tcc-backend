using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Application.Services;

namespace TccSharkTank.WebApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "adm")]
public sealed class AdminController : ControllerBase
{
    private readonly IDashboardService _dashboard;
    private readonly IGovernancaService _governanca;

    public AdminController(IDashboardService dashboard, IGovernancaService governanca)
    {
        _dashboard = dashboard;
        _governanca = governanca;
    }

    [HttpGet("dashboard")]
    public Task<DashboardAdminResponse> GetDashboard(CancellationToken cancellationToken)
        => _dashboard.GetAdminDashboardAsync(cancellationToken);

    [HttpGet("denuncias")]
    public Task<List<DenunciaResponse>> ListarDenuncias(CancellationToken cancellationToken)
        => _governanca.ListarDenunciasAsync(cancellationToken);

    [HttpPost("denuncias/{id:long}/analisar")]
    public Task<DenunciaResponse> AnalisarDenuncia([FromRoute] long id, [FromBody] AnalisarDenunciaRequest request, CancellationToken cancellationToken)
        => _governanca.AnalisarDenunciaAsync(id, request, cancellationToken);
}

[ApiController]
[Route("api/governança")]
[Authorize]
public sealed class GovernancaController : ControllerBase
{
    private readonly IGovernancaService _governanca;
    private readonly ICurrentUser _currentUser;

    public GovernancaController(IGovernancaService governanca, ICurrentUser currentUser)
    {
        _governanca = governanca;
        _currentUser = currentUser;
    }

    [HttpPost("denunciar")]
    public Task<DenunciaResponse> Denunciar([FromBody] CreateDenunciaRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _governanca.DenunciarAsync(userId, request, cancellationToken);
    }
}
