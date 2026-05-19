using TccSharkTank.Application.Abstractions.Persistence;
using TccSharkTank.Application.Abstractions.System;
using TccSharkTank.Application.Common;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Application.Services;

public sealed record PagamentoResponse(
    long Id,
    decimal Valor,
    string Descricao,
    string Metodo,
    string Status,
    DateTime CreateDate
);

public sealed record SimularPagamentoRequest(
    decimal Valor,
    string Descricao
);

public interface IPagamentoService
{
    Task<PagamentoResponse> SimularPagamentoAsync(long usuarioId, SimularPagamentoRequest request, CancellationToken cancellationToken);
    Task<List<PagamentoResponse>> ListarMeusPagamentosAsync(long usuarioId, CancellationToken cancellationToken);
}

public sealed class PagamentoService : IPagamentoService
{
    private readonly IPagamentoRepository _pagamentos;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly ILogService _logs;

    public PagamentoService(IPagamentoRepository pagamentos, IUnitOfWork uow, IClock clock, ILogService logs)
    {
        _pagamentos = pagamentos;
        _uow = uow;
        _clock = clock;
        _logs = logs;
    }

    public async Task<PagamentoResponse> SimularPagamentoAsync(long usuarioId, SimularPagamentoRequest request, CancellationToken cancellationToken)
    {
        var pagamento = new PgtPagamento
        {
            Id = 0,
            UsuarioId = usuarioId,
            Valor = request.Valor,
            Descricao = request.Descricao,
            Metodo = "Cartão (Simulado)",
            Status = "Aprovado",
            CreateDate = _clock.UtcNow,
            UpdateDate = _clock.UtcNow
        };

        await _pagamentos.AddAsync(pagamento, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _logs.RegistrarAsync("pagamento", usuarioId, null, null, $"Pagamento simulado de {request.Valor:C2}: {request.Descricao}", cancellationToken);

        return Map(pagamento);
    }

    public async Task<List<PagamentoResponse>> ListarMeusPagamentosAsync(long usuarioId, CancellationToken cancellationToken)
    {
        var pagamentos = await _pagamentos.ListByUsuarioAsync(usuarioId, cancellationToken);
        return pagamentos.Select(Map).ToList();
    }

    private static PagamentoResponse Map(PgtPagamento p) => new(
        Id: p.Id,
        Valor: p.Valor,
        Descricao: p.Descricao,
        Metodo: p.Metodo,
        Status: p.Status,
        CreateDate: p.CreateDate
    );
}
