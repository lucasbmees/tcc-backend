using TccSharkTank.Application.Abstractions.Persistence;
using TccSharkTank.Application.Common;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Application.Services;

public interface IJuridicoService
{
    Task<string> GerarTermoInvestimentoAsync(long propostaId, long usuarioId, CancellationToken cancellationToken);
}

public sealed class JuridicoService : IJuridicoService
{
    private readonly IPropostaRepository _propostas;
    private readonly IIdeiaRepository _ideias;
    private readonly IUsuarioRepository _usuarios;

    public JuridicoService(
        IPropostaRepository propostas,
        IIdeiaRepository ideias,
        IUsuarioRepository usuarios)
    {
        _propostas = propostas;
        _ideias = ideias;
        _usuarios = usuarios;
    }

    public async Task<string> GerarTermoInvestimentoAsync(long propostaId, long usuarioId, CancellationToken cancellationToken)
    {
        var proposta = await _propostas.GetByIdAsync(propostaId, cancellationToken);
        if (proposta is null) throw new AppException("Proposta não encontrada.", 404);

        var ideia = await _ideias.GetByIdAsync(proposta.IdeiaId, cancellationToken);
        if (ideia is null) throw new AppException("Ideia não encontrada.", 404);

        var aceita = proposta.Infos.OrderByDescending(i => i.CreateDate).FirstOrDefault(i => i.AceiteId == 1);
        if (aceita is null)
        {
            throw new AppException("Contrato disponível apenas após a proposta ser aceita.", 403);
        }

        var termos = proposta.Infos
            .Where(i => i.Valor > 0m && i.FatiaPret > 0m)
            .OrderByDescending(i => i.CreateDate)
            .FirstOrDefault()
            ?? proposta.Infos.OrderBy(i => i.CreateDate).FirstOrDefault();

        if (termos is null)
        {
            throw new AppException("Proposta sem informações para gerar contrato.", 500);
        }

        var equityPercent = termos.FatiaPret <= 1m ? termos.FatiaPret * 100m : termos.FatiaPret;

        if (usuarioId != proposta.UsuarioId && usuarioId != ideia.UsuarioId)
        {
            throw new AppException("Sem permissão para acessar este contrato.", 403);
        }

        var investidor = await _usuarios.GetByIdAsync(proposta.UsuarioId, cancellationToken);
        var empreendedor = await _usuarios.GetByIdAsync(ideia.UsuarioId, cancellationToken);
        
        // Simulação de um contrato jurídico básico
        return $@"
# TERMO DE COMPROMISSO DE INVESTIMENTO (MÚTUO CONVERSÍVEL)

Pelo presente instrumento particular, as partes abaixo qualificadas:

**INVESTIDOR:** {investidor?.Nome} {investidor?.Sobrenome}, CPF/CNPJ: {investidor?.Email}
**EMPREENDEDOR:** {empreendedor?.Nome} {empreendedor?.Sobrenome}, CPF/CNPJ: {empreendedor?.Email}
**STARTUP (IDEIA):** {ideia.Nome}, CNPJ: {ideia.Info?.Cnpj}

Resolvem celebrar este Termo de Compromisso de Investimento, mediante as seguintes cláusulas:

1. **OBJETO:** O Investidor compromete-se a aportar o valor de {termos.Valor:C2} na Startup, em troca de uma participação societária de {equityPercent:0.##}% (Equity).
2. **CONDIÇÕES:** O aporte será realizado após a formalização final dos documentos societários, no prazo de 30 dias.
3. **VALIDADE:** Este termo passa a valer a partir do aceite digital realizado na plataforma TCC Shark Tank em {proposta.UpdateDate:dd/MM/yyyy HH:mm}.

As partes reconhecem a validade do aceite digital como manifestação de vontade irrevogável.

Data de Geração: {DateTime.Now:dd/MM/yyyy}
Autenticação: TCCST-{proposta.Id}-{DateTime.Now.Ticks}
";
    }
}
