using TccSharkTank.Application.Abstractions.Persistence;

namespace TccSharkTank.Application.Services;

public sealed record DashboardAdminResponse(
    decimal TotalInvestimentoProposto,
    int TotalStartups,
    double TaxaConversao,
    List<GraficoVolumeResponse> VolumeStartupsMensal
);

public sealed record GraficoVolumeResponse(string Mes, int Quantidade);

public interface IDashboardService
{
    Task<DashboardAdminResponse> GetAdminDashboardAsync(CancellationToken cancellationToken);
}

public sealed class DashboardService : IDashboardService
{
    private readonly IIdeiaRepository _ideias;
    private readonly IPropostaRepository _propostas;

    public DashboardService(IIdeiaRepository ideias, IPropostaRepository propostas)
    {
        _ideias = ideias;
        _propostas = propostas;
    }

    public async Task<DashboardAdminResponse> GetAdminDashboardAsync(CancellationToken cancellationToken)
    {
        var todasPropostas = await _propostas.ListTodasAsync(cancellationToken);
        var totalInvestimento = todasPropostas
            .SelectMany(p => p.Infos)
            .Where(i => i.AceiteId == 1) // Apenas aceitas
            .Sum(i => i.Valor);

        var totalIdeias = await _ideias.CountAsync(cancellationToken);

        // Taxa de conversão: propostas aceitas / propostas totais
        var propostasAceitas = todasPropostas.Count(p => p.Infos.Any(i => i.AceiteId == 1));
        var propostasTotais = todasPropostas.Count;
        var taxaConversao = propostasTotais > 0 ? (double)propostasAceitas / propostasTotais * 100 : 0;

        // Volume por mês (últimos 6 meses)
        // Como as ideias são AuditableEntityBase, elas têm CreateDate
        var ideias = await _ideias.ListAsync(null, null, null, null, null, null, cancellationToken);
        var volumeMensal = ideias
            .GroupBy(i => new { i.CreateDate.Year, i.CreateDate.Month })
            .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
            .Take(6)
            .Select(g => new GraficoVolumeResponse($"{g.Key.Month}/{g.Key.Year}", g.Count()))
            .Reverse()
            .ToList();

        return new DashboardAdminResponse(
            TotalInvestimentoProposto: totalInvestimento,
            TotalStartups: totalIdeias,
            TaxaConversao: Math.Round(taxaConversao, 2),
            VolumeStartupsMensal: volumeMensal
        );
    }
}
