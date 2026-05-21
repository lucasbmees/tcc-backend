using TccSharkTank.Application.Abstractions.Persistence;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Application.Abstractions.System;
using TccSharkTank.Application.Common;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Application.Services;

public interface IPlanoService
{
    Task<List<PlanoInfoResponse>> ListarAsync(CancellationToken cancellationToken);
    Task<PlanoMeuResponse> MeuAsync(long usuarioId, CancellationToken cancellationToken);
    Task<AssinarPlanoResponse> AssinarAsync(long usuarioId, AssinarPlanoRequest request, CancellationToken cancellationToken);
}

public sealed class PlanoService : IPlanoService
{
    private const int PlanoBasicoId = 1;
    private const int PlanoProId = 2;
    private const int PlanoEliteId = 3;

    private readonly IUsuarioRepository _usuarios;
    private readonly IPagamentoRepository _pagamentos;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly ILogService _logs;
    private readonly IJwtTokenService _jwt;

    public PlanoService(
        IUsuarioRepository usuarios,
        IPagamentoRepository pagamentos,
        IUnitOfWork uow,
        IClock clock,
        ILogService logs,
        IJwtTokenService jwt)
    {
        _usuarios = usuarios;
        _pagamentos = pagamentos;
        _uow = uow;
        _clock = clock;
        _logs = logs;
        _jwt = jwt;
    }

    public Task<List<PlanoInfoResponse>> ListarAsync(CancellationToken cancellationToken)
    {
        var planos = new List<PlanoInfoResponse>
        {
            new(PlanoBasicoId, "Básico", "basico"),
            new(PlanoProId, "Pro", "pro"),
            new(PlanoEliteId, "Elite", "elite"),
        };

        return Task.FromResult(planos);
    }

    public async Task<PlanoMeuResponse> MeuAsync(long usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new AppException("Usuário não encontrado.", 404);

        var (plano, regalias) = GetPlanoERegalias(usuario);
        return new PlanoMeuResponse(plano, regalias);
    }

    public async Task<AssinarPlanoResponse> AssinarAsync(long usuarioId, AssinarPlanoRequest request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new AppException("Usuário não encontrado.", 404);

        var cargo = (usuario.Cargo?.Nome ?? string.Empty).ToLowerInvariant();
        var codigo = (request.PlanoCodigo ?? string.Empty).Trim().ToLowerInvariant();

        var novoPlanoId = codigo switch
        {
            "basico" => PlanoBasicoId,
            "pro" => PlanoProId,
            "elite" => PlanoEliteId,
            _ => throw new AppException("Plano inválido.", 400),
        };

        if (novoPlanoId == PlanoProId && cargo != "empreendedor")
            throw new AppException("O plano Pro é exclusivo para empreendedores.", 403);

        if (novoPlanoId == PlanoEliteId && cargo != "investidor")
            throw new AppException("O plano Elite é exclusivo para investidores.", 403);

        decimal valor = novoPlanoId switch
        {
            PlanoProId => 49.90m,
            PlanoEliteId => 199.90m,
            _ => 0m
        };

        if (novoPlanoId != PlanoBasicoId)
        {
            var pagamento = new PgtPagamento
            {
                Id = 0,
                UsuarioId = usuarioId,
                Valor = valor,
                Descricao = $"Assinatura Plano {codigo}",
                Metodo = "Cartão (Simulado)",
                Status = "Aprovado",
                CreateDate = _clock.UtcNow,
                UpdateDate = _clock.UtcNow
            };

            await _pagamentos.AddAsync(pagamento, cancellationToken);
        }

        usuario.PlanoId = novoPlanoId;
        _usuarios.Update(usuario);
        await _uow.SaveChangesAsync(cancellationToken);

        await _logs.RegistrarAsync(
            tipoNome: "plano",
            usuarioId: usuarioId,
            ideiaId: null,
            propostaId: null,
            descricao: $"Alteração de plano para {codigo}",
            cancellationToken: cancellationToken);

        var atualizado = await _usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new AppException("Usuário não encontrado.", 404);

        var (plano, regalias) = GetPlanoERegalias(atualizado);
        var token = _jwt.GenerateToken(atualizado);
        return new AssinarPlanoResponse($"Plano atualizado para {plano.Nome}.", plano, token);
    }

    private static (PlanoInfoResponse Plano, List<string> Regalias) GetPlanoERegalias(UsuUsuario usuario)
    {
        var cargo = (usuario.Cargo?.Nome ?? string.Empty).ToLowerInvariant();
        var planoId = usuario.PlanoId;
        var planoCodigo = usuario.Plano?.Nome ?? (planoId switch { PlanoProId => "pro", PlanoEliteId => "elite", _ => "basico" });

        var planoNome = planoCodigo switch
        {
            "pro" => "Pro",
            "elite" => "Elite",
            _ => "Básico"
        };

        var regalias = new List<string>();

        if (planoCodigo == "pro")
        {
            regalias.Add("Ideias ilimitadas (empreendedor)");
            regalias.Add("Destaque nas buscas");
        }

        if (planoCodigo == "elite")
        {
            regalias.Add("Selo de investidor verificado");
            regalias.Add("Relatório de due diligence (download)");
            regalias.Add("Filtro: somente ideias com documentos");
            regalias.Add("Prioridade nas propostas (empreendedor vê primeiro)");
            regalias.Add("Relatórios jurídicos (download de contrato)");
        }

        if (planoCodigo == "basico")
        {
            if (cargo == "empreendedor") regalias.Add("Até 2 ideias ativas");
        }

        return (new PlanoInfoResponse(planoId, planoNome, planoCodigo), regalias);
    }
}
