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
    private readonly IRelatorioService _relatorios;

    public IdeiasController(IIdeiaService ideias, ICurrentUser currentUser, IRelatorioService relatorios)
    {
        _ideias = ideias;
        _currentUser = currentUser;
        _relatorios = relatorios;
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
    public Task<List<IdeiaDetailsResponse>> List(
        [FromQuery] string? termo,
        [FromQuery] int? categoriaId,
        [FromQuery] int? estagioId,
        [FromQuery] string? regiao,
        [FromQuery] decimal? valorMin,
        [FromQuery] decimal? valorMax,
        [FromQuery] bool? apenasComDocumentos,
        CancellationToken cancellationToken)
    {
        if (apenasComDocumentos == true)
        {
            var role = (_currentUser.Role ?? string.Empty).ToLowerInvariant();
            var plan = (User.FindFirst("plan")?.Value ?? string.Empty).ToLowerInvariant();
            if (role != "adm" && (role != "investidor" || plan != "elite"))
                throw new TccSharkTank.Application.Common.AppException("Filtro disponível apenas para investidores Elite.", 403);
        }

        return _ideias.ListarAsync(termo, categoriaId, estagioId, regiao, valorMin, valorMax, apenasComDocumentos, cancellationToken);
    }

    [Authorize]
    [HttpGet("{id:long}/relatorio")]
    public async Task<IActionResult> Relatorio([FromRoute] long id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        var role = (_currentUser.Role ?? string.Empty).ToLowerInvariant();
        var plan = (User.FindFirst("plan")?.Value ?? string.Empty).ToLowerInvariant();

        if (role != "adm" && (role != "investidor" || plan != "elite"))
            throw new TccSharkTank.Application.Common.AppException("Recurso disponível apenas para investidores Elite.", 403);

        var conteudo = await _relatorios.GerarRelatorioIdeiaAsync(id, cancellationToken);
        var bytes = System.Text.Encoding.UTF8.GetBytes(conteudo);
        return File(bytes, "text/markdown", $"Relatorio-Ideia-{id}.md");
    }

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

    [Authorize]
    [HttpPost("{id:long}/comentarios")]
    public Task<ComentarioResponse> PostComentario([FromRoute] long id, [FromBody] CreateComentarioRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _ideias.ComentarAsync(id, userId, request, cancellationToken);
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
