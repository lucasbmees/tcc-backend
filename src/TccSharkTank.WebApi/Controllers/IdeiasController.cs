using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Application.Services;

namespace TccSharkTank.WebApi.Controllers;

[ApiController]
[Route("api/ideias")]
public sealed class IdeiasController : ControllerBase
{
    private readonly IIdeiaService _ideias;
    private readonly ICurrentUser _currentUser;

    public IdeiasController(IIdeiaService ideias, ICurrentUser currentUser)
    {
        _ideias = ideias;
        _currentUser = currentUser;
    }

    [Authorize(Roles = "empreendedor")]
    [HttpPost]
    public Task<IdeiaDetailsResponse> Create([FromBody] CreateIdeiaRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _ideias.CadastrarAsync(userId, request, cancellationToken);
    }

    [AllowAnonymous]
    [HttpGet]
    public Task<List<IdeiaDetailsResponse>> List([FromQuery] int? categoriaId, CancellationToken cancellationToken)
        => _ideias.ListarAsync(categoriaId, cancellationToken);

    [AllowAnonymous]
    [HttpGet("{id:long}")]
    public Task<IdeiaDetailsResponse> Details([FromRoute] long id, CancellationToken cancellationToken)
        => _ideias.DetalhesAsync(id, cancellationToken);

    [Authorize(Roles = "empreendedor")]
    [HttpPut("{id:long}")]
    public Task<IdeiaDetailsResponse> Update([FromRoute] long id, [FromBody] UpdateIdeiaRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _ideias.EditarAsync(id, userId, request, cancellationToken);
    }

    [Authorize(Roles = "empreendedor")]
    [HttpPost("{id:long}/documentos")]
    public async Task<IdeiaDocumentoResponse> UploadDocumento([FromRoute] long id, IFormFile arquivo, CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            throw new TccSharkTank.Application.Common.AppException("Arquivo inválido.", 400);
        }

        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);

        await using var stream = arquivo.OpenReadStream();
        return await _ideias.UploadDocumentoAsync(id, userId, stream, arquivo.FileName, cancellationToken);
    }
}

[ApiController]
[Route("api/admin/ideias")]
[Authorize(Roles = "adm")]
public sealed class AdminIdeiasController : ControllerBase
{
    private readonly IIdeiaService _ideias;

    public AdminIdeiasController(IIdeiaService ideias)
    {
        _ideias = ideias;
    }

    [HttpPatch("{id:long}/status")]
    public Task<IdeiaDetailsResponse> ChangeStatus([FromRoute] long id, [FromBody] ChangeIdeiaStatusRequest request, CancellationToken cancellationToken)
        => _ideias.AlterarStatusAsync(id, request, cancellationToken);
}
