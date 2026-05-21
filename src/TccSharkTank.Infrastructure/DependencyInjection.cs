using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TccSharkTank.Application.Abstractions.Persistence;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Application.Abstractions.System;
using TccSharkTank.Application.Services;
using TccSharkTank.Infrastructure.Persistence;
using TccSharkTank.Infrastructure.Security;
using TccSharkTank.Infrastructure.System;

namespace TccSharkTank.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Data Source=tcc_sharktank.db";
        }

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICargoRepository, CargoRepository>();
        services.AddScoped<IIdeiaRepository, IdeiaRepository>();
        services.AddScoped<IPropostaRepository, EfPropostaRepository>();
        services.AddScoped<IChatRepository, EfChatRepository>();
        services.AddScoped<IPagamentoRepository, EfPagamentoRepository>();
        services.AddScoped<IGovernancaRepository, EfGovernancaRepository>();
        services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
        services.AddScoped<ILogRepository, LogRepository>();
        services.AddScoped<ILookupRepository, LookupRepository>();

        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IEmailService, SimuladoEmailService>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IIdeiaService, IdeiaService>();
        services.AddScoped<IPropostaService, PropostaService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IJuridicoService, JuridicoService>();
        services.AddScoped<IPagamentoService, PagamentoService>();
        services.AddScoped<IPlanoService, PlanoService>();
        services.AddScoped<IRelatorioService, RelatorioService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IGovernancaService, GovernancaService>();
        services.AddScoped<INotificacaoService, NotificacaoService>();

        return services;
    }
}
