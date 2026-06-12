using TccSharkTank.Application.Abstractions.Persistence;
using TccSharkTank.Application.Abstractions.System;
using TccSharkTank.Application.Common;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Application.Services;

public sealed record DenunciaResponse(
    long Id,
    long DenuncianteId,
    string DenuncianteNome,
    string TipoAlvo,
    long AlvoId,
    string Motivo,
    string Status,
    string? ObservacaoAdm,
    DateTime CreateDate
);

public sealed record CreateDenunciaRequest(
    string TipoAlvo,
    long AlvoId,
    string Motivo
);

public sealed record AnalisarDenunciaRequest(
    string Status,
    string? ObservacaoAdm
);

public interface IGovernancaService
{
    Task<DenunciaResponse> DenunciarAsync(long denuncianteId, CreateDenunciaRequest request, CancellationToken cancellationToken);
    Task<List<DenunciaResponse>> ListarDenunciasAsync(CancellationToken cancellationToken);
    Task<DenunciaResponse> AnalisarDenunciaAsync(long denunciaId, AnalisarDenunciaRequest request, CancellationToken cancellationToken);
}

public sealed class GovernancaService : IGovernancaService
{
    private readonly IGovernancaRepository _governanca;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly ILogService _logs;

    public GovernancaService(IGovernancaRepository governanca, IUnitOfWork uow, IClock clock, ILogService logs)
    {
        _governanca = governanca;
        _uow = uow;
        _clock = clock;
        _logs = logs;
    }

    public async Task<DenunciaResponse> DenunciarAsync(long denuncianteId, CreateDenunciaRequest request, CancellationToken cancellationToken)
    {
        var denuncia = new GovDenuncia
        {
            Id = 0,
            DenuncianteId = denuncianteId,
            TipoAlvo = request.TipoAlvo,
            AlvoId = request.AlvoId,
            Motivo = request.Motivo.Trim(),
            Status = "Pendente",
            CreateDate = _clock.UtcNow,
            UpdateDate = _clock.UtcNow
        };

        await _governanca.AddDenunciaAsync(denuncia, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _logs.RegistrarAsync("governança", denuncianteId, null, null, $"Nova denúncia de {request.TipoAlvo} #{request.AlvoId}", cancellationToken);

        return Map(denuncia);
    }

    public async Task<List<DenunciaResponse>> ListarDenunciasAsync(CancellationToken cancellationToken)
    {
        var denuncias = await _governanca.ListDenunciasAsync(cancellationToken);
        return denuncias.Select(Map).ToList();
    }

    public async Task<DenunciaResponse> AnalisarDenunciaAsync(long denunciaId, AnalisarDenunciaRequest request, CancellationToken cancellationToken)
    {
        var denuncia = await _governanca.GetDenunciaByIdAsync(denunciaId, cancellationToken);
        if (denuncia is null) throw new AppException("Denúncia não encontrada.", 404);

        denuncia.Status = request.Status;
        denuncia.ObservacaoAdm = request.ObservacaoAdm;
        denuncia.UpdateDate = _clock.UtcNow;

        _governanca.UpdateDenuncia(denuncia);
        await _uow.SaveChangesAsync(cancellationToken);

        return Map(denuncia);
    }

    private static DenunciaResponse Map(GovDenuncia d) => new(
        Id: d.Id,
        DenuncianteId: d.DenuncianteId,
        DenuncianteNome: d.Denunciante != null ? $"{d.Denunciante.Nome} {d.Denunciante.Sobrenome}" : "Usuário",
        TipoAlvo: d.TipoAlvo,
        AlvoId: d.AlvoId,
        Motivo: d.Motivo,
        Status: d.Status,
        ObservacaoAdm: d.ObservacaoAdm,
        CreateDate: d.CreateDate
    );
}
