using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TccSharkTank.Application.Services;

namespace TccSharkTank.WebApi.Controllers;

public sealed record RegistrarLogRequest(string Tipo, long? UsuarioId, long? IdeiaId, long? PropostaId, string Descricao);

[ApiController]
[Route("api/logs")]
[Authorize(Roles = "adm")]
public sealed class LogsController : ControllerBase
{
    private readonly ILogService _logs;

    public LogsController(ILogService logs)
    {
        _logs = logs;
    }

    [HttpPost]
    public Task Registrar([FromBody] RegistrarLogRequest request, CancellationToken cancellationToken)
        => _logs.RegistrarAsync(request.Tipo, request.UsuarioId, request.IdeiaId, request.PropostaId, request.Descricao, cancellationToken);
}

