using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Application.Services;
using TccSharkTank.Infrastructure.Persistence;

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

    private IdeiaDetailsResponse ApplyDocumentoVisibilidade(IdeiaDetailsResponse ideia)
    {
        var userId = _currentUser.UserId;
        var role = (_currentUser.Role ?? string.Empty).ToLowerInvariant();
        var plan = (User.FindFirst("plan")?.Value ?? string.Empty).ToLowerInvariant();

        var isOwner = userId.HasValue && ideia.IdaUsuarioId == userId.Value;
        var isPaid = plan is "elite" or "pro";

        if (role == "adm" || isOwner || isPaid)
        {
            return ideia;
        }

        return new IdeiaDetailsResponse(
            IdaId: ideia.IdaId,
            IdaUsuarioId: ideia.IdaUsuarioId,
            IdaNome: ideia.IdaNome,
            Regiao: ideia.Regiao,
            IdaCategoriaId: ideia.IdaCategoriaId,
            CategoriaNome: ideia.CategoriaNome,
            IdaEstagioId: ideia.IdaEstagioId,
            EstagioNome: ideia.EstagioNome,
            IdaStatusId: ideia.IdaStatusId,
            StatusNome: ideia.StatusNome,
            IdaMotivoStatus: ideia.IdaMotivoStatus,
            Info: ideia.Info,
            Documentos: new List<IdeiaDocumentoResponse>(),
            Comentarios: ideia.Comentarios
        );
    }

    private static byte[] BuildSimplePdf(string text)
    {
        static string Escape(string s) => s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        var safe = Escape(text);
        var content = $"BT /F1 18 Tf 50 100 Td ({safe}) Tj ET";
        var contentBytes = System.Text.Encoding.ASCII.GetBytes(content);

        var header = "%PDF-1.4\n";

        var obj1 = "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n";
        var obj2 = "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n";
        var obj3 = "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n";
        var obj4Prefix = $"4 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n";
        var obj4Suffix = "\nendstream\nendobj\n";
        var obj5 = "5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n";

        var parts = new List<byte[]>
        {
            System.Text.Encoding.ASCII.GetBytes(header),
            System.Text.Encoding.ASCII.GetBytes(obj1),
            System.Text.Encoding.ASCII.GetBytes(obj2),
            System.Text.Encoding.ASCII.GetBytes(obj3),
            System.Text.Encoding.ASCII.GetBytes(obj4Prefix),
            contentBytes,
            System.Text.Encoding.ASCII.GetBytes(obj4Suffix),
            System.Text.Encoding.ASCII.GetBytes(obj5),
        };

        var offsets = new List<int> { 0 };
        var pos = parts[0].Length;

        offsets.Add(pos);
        pos += parts[1].Length;
        offsets.Add(pos);
        pos += parts[2].Length;
        offsets.Add(pos);
        pos += parts[3].Length;
        offsets.Add(pos);
        pos += parts[4].Length + parts[5].Length + parts[6].Length;
        offsets.Add(pos);
        pos += parts[7].Length;

        var sb = new System.Text.StringBuilder();
        sb.Append("xref\n");
        sb.Append("0 6\n");
        sb.Append("0000000000 65535 f \n");
        for (var i = 1; i <= 5; i++)
        {
            sb.Append(offsets[i].ToString("D10")).Append(" 00000 n \n");
        }

        var xref = sb.ToString();
        var xrefBytes = System.Text.Encoding.ASCII.GetBytes(xref);
        var trailer = "trailer\n<< /Size 6 /Root 1 0 R >>\n";
        var trailerBytes = System.Text.Encoding.ASCII.GetBytes(trailer);

        var startXrefPos = pos;
        var startXref = $"startxref\n{startXrefPos}\n%%EOF\n";
        var startXrefBytes = System.Text.Encoding.ASCII.GetBytes(startXref);

        var totalLen = pos + xrefBytes.Length + trailerBytes.Length + startXrefBytes.Length;
        var output = new byte[totalLen];
        var o = 0;
        foreach (var p in parts)
        {
            Buffer.BlockCopy(p, 0, output, o, p.Length);
            o += p.Length;
        }
        Buffer.BlockCopy(xrefBytes, 0, output, o, xrefBytes.Length);
        o += xrefBytes.Length;
        Buffer.BlockCopy(trailerBytes, 0, output, o, trailerBytes.Length);
        o += trailerBytes.Length;
        Buffer.BlockCopy(startXrefBytes, 0, output, o, startXrefBytes.Length);

        return output;
    }

    [Authorize(Roles = "empreendedor")]
    [HttpPost]
    public Task<IdeiaDetailsResponse> Create([FromBody] CreateIdeiaRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        return _ideias.CadastrarAsync(userId, request, cancellationToken);
    }

    [Authorize]
    [HttpGet]
    public async Task<List<IdeiaDetailsResponse>> List(
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

        var ideias = await _ideias.ListarAsync(termo, categoriaId, estagioId, regiao, valorMin, valorMax, apenasComDocumentos, cancellationToken);
        return ideias.Select(ApplyDocumentoVisibilidade).ToList();
    }

    // =================================================================
    // NOVO MÉTODO DA BUSCA INTELIGENTE (IA) 
    // =================================================================
    [Authorize]
    [HttpGet("busca-ia")]
    public async Task<List<IdeiaDetailsResponse>> BuscaInteligente(
        [FromQuery] string termo, 
        [FromServices] IGeminiService gemini,
        CancellationToken cancellationToken)
    {
        var todasAsIdeias = await _ideias.ListarAsync(null, null, null, null, null, null, null, cancellationToken);
        
        if (string.IsNullOrWhiteSpace(termo) || !todasAsIdeias.Any())
            return todasAsIdeias.Select(ApplyDocumentoVisibilidade).ToList(); 

        // Contexto corrigido com IdaId, IdaNome e IdaDescricao
        var contexto = string.Join(" | ", todasAsIdeias.Select(i => $"[{i.IdaId} - {i.IdaNome} - {i.Info?.IdaInfoDescricao}]"));
        var idsFiltrados = await gemini.FiltrarIdeiasComIA(termo, contexto, cancellationToken);

        // Filtro do return corrigido com IdaId
        return todasAsIdeias.Where(i => idsFiltrados.Contains(i.IdaId)).Select(ApplyDocumentoVisibilidade).ToList();
    }
    // =================================================================

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

    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<IdeiaDetailsResponse> Details([FromRoute] long id, CancellationToken cancellationToken)
    {
        var ideia = await _ideias.DetalhesAsync(id, cancellationToken);
        return ApplyDocumentoVisibilidade(ideia);
    }

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
        var doc = await _ideias.UploadDocumentoAsync(id, userId, stream, arquivo.FileName, cancellationToken);
        return new IdeiaDocumentoResponse(doc.IdaDocumentoId, $"/api/ideias/{id}/documentos/{doc.IdaDocumentoId}/download");
    }

    [Authorize]
    [HttpGet("{id:long}/documentos/{docId:long}/download")]
    public async Task<IActionResult> DownloadDocumento([FromRoute] long id, [FromRoute] long docId, [FromServices] AppDbContext db, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new TccSharkTank.Application.Common.AppException("Não autenticado.", 401);
        var role = (_currentUser.Role ?? string.Empty).ToLowerInvariant();
        var plan = (User.FindFirst("plan")?.Value ?? string.Empty).ToLowerInvariant();

        var ideia = await db.IdaIdeias
            .Include(i => i.Documentos)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (ideia is null)
        {
            throw new TccSharkTank.Application.Common.AppException("Ideia não encontrada.", 404);
        }

        var isOwner = ideia.UsuarioId == userId;
        var isPaid = plan is "elite" or "pro";
        if (role != "adm" && !isOwner && !isPaid)
        {
            throw new TccSharkTank.Application.Common.AppException("Recurso disponível apenas para planos pagos.", 403);
        }

        var doc = ideia.Documentos.FirstOrDefault(d => d.Id == docId);
        if (doc is null)
        {
            throw new TccSharkTank.Application.Common.AppException("Documento não encontrado.", 404);
        }

        if (!System.IO.File.Exists(doc.Arquivo))
        {
            var pdf = BuildSimplePdf("Documento indisponível no ambiente atual.");
            return File(pdf, "application/pdf", $"Ideia-{id}-Documento-{docId}.pdf");
        }

        var stream = new FileStream(doc.Arquivo, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, "application/pdf", $"Ideia-{id}-Documento-{docId}.pdf");
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
