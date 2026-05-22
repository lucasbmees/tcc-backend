using System.Text;
using TccSharkTank.Application.Abstractions.Persistence;
using TccSharkTank.Application.Common;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Application.Services;

public interface IRelatorioService
{
    Task<string> GerarRelatorioIdeiaAsync(long ideiaId, CancellationToken cancellationToken);
}

public sealed class RelatorioService : IRelatorioService
{
    private readonly IIdeiaRepository _ideias;
    private readonly IPropostaRepository _propostas;

    public RelatorioService(IIdeiaRepository ideias, IPropostaRepository propostas)
    {
        _ideias = ideias;
        _propostas = propostas;
    }

    public async Task<string> GerarRelatorioIdeiaAsync(long ideiaId, CancellationToken cancellationToken)
    {
        var ideia = await _ideias.GetByIdAsync(ideiaId, cancellationToken)
            ?? throw new AppException("Ideia não encontrada.", 404);

        var propostas = await _propostas.ListByIdeiaAsync(ideiaId, cancellationToken);
        var total = propostas.Count;

        var aceita = 0;
        var recusada = 0;
        var pendente = 0;
        var contraproposta = 0;

        foreach (var p in propostas)
        {
            var ultima = p.Infos.OrderByDescending(i => i.CreateDate).FirstOrDefault();
            switch (ultima?.AceiteId)
            {
                case 1: aceita++; break;
                case 2: recusada++; break;
                case 4: contraproposta++; break;
                default: pendente++; break;
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("# RELATÓRIO (ELITE) — Due Diligence Simulada");
        sb.AppendLine();
        sb.AppendLine($"**Ideia:** {ideia.Nome}");
        sb.AppendLine($"**Empreendedor (ID):** {ideia.UsuarioId}");
        sb.AppendLine($"**Categoria:** {ideia.Categoria?.Nome}");
        sb.AppendLine($"**Estágio:** {ideia.Estagio?.Nome}");
        sb.AppendLine($"**Status:** {ideia.Status?.Nome}");
        sb.AppendLine($"**Região:** {ideia.Regiao ?? "-"}");
        sb.AppendLine();

        if (ideia.Info != null)
        {
            sb.AppendLine("## Informações");
            sb.AppendLine();
            sb.AppendLine($"- **CNPJ:** {ideia.Info.Cnpj}");
            sb.AppendLine($"- **Captação:** {ideia.Info.ValorCaptacao:C2}");
            sb.AppendLine($"- **Fatia ofertada:** {ideia.Info.Fatia:0.##}%");
            if (!string.IsNullOrWhiteSpace(ideia.Info.LinkVideo)) sb.AppendLine($"- **Vídeo:** {ideia.Info.LinkVideo}");
            sb.AppendLine();
        }

        sb.AppendLine("## Documentos");
        sb.AppendLine();
        if (ideia.Documentos.Count == 0)
        {
            sb.AppendLine("- Nenhum documento enviado.");
        }
        else
        {
            foreach (var doc in ideia.Documentos.OrderBy(d => d.Id))
            {
                sb.AppendLine($"- Documento #{doc.Id}: {doc.Arquivo}");
            }
        }
        sb.AppendLine();

        sb.AppendLine("## Propostas (resumo)");
        sb.AppendLine();
        sb.AppendLine($"- **Total:** {total}");
        sb.AppendLine($"- **Aceitas:** {aceita}");
        sb.AppendLine($"- **Recusadas:** {recusada}");
        sb.AppendLine($"- **Pendentes:** {pendente}");
        sb.AppendLine($"- **Contrapropostas:** {contraproposta}");
        sb.AppendLine();

        var top = propostas
            .Select(p => new { Proposta = p, Ultima = p.Infos.OrderByDescending(i => i.CreateDate).FirstOrDefault() })
            .Where(x => x.Ultima != null)
            .OrderByDescending(x => x.Ultima!.Valor)
            .Take(3)
            .ToList();

        sb.AppendLine("## Top 3 propostas por valor");
        sb.AppendLine();
        if (top.Count == 0)
        {
            sb.AppendLine("- Sem propostas.");
        }
        else
        {
            foreach (var item in top)
            {
                sb.AppendLine($"- Proposta #{item.Proposta.Id} (Investidor {item.Proposta.UsuarioId}): {item.Ultima!.Valor:C2} por {item.Ultima.FatiaPret:0.##}%");
            }
        }

        return sb.ToString();
    }
}

