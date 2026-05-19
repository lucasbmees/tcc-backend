using Microsoft.EntityFrameworkCore;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UsuUsuario> UsuUsuarios => Set<UsuUsuario>();
    public DbSet<UsuCargo> UsuCargos => Set<UsuCargo>();
    public DbSet<UsuPerfil> UsuPerfis => Set<UsuPerfil>();

    public DbSet<IdaIdeia> IdaIdeias => Set<IdaIdeia>();
    public DbSet<IdaStatus> IdaStatuses => Set<IdaStatus>();
    public DbSet<IdaEstagio> IdaEstagios => Set<IdaEstagio>();
    public DbSet<IdaCategoria> IdaCategorias => Set<IdaCategoria>();
    public DbSet<IdaComentario> IdaComentarios => Set<IdaComentario>();
    public DbSet<IdaDocumento> IdaDocumentos => Set<IdaDocumento>();
    public DbSet<IdaInfo> IdaInfos => Set<IdaInfo>();

    public DbSet<ChtConversa> ChtConversas => Set<ChtConversa>();
    public DbSet<ChtMensagem> ChtMensagens => Set<ChtMensagem>();

    public DbSet<PgtPagamento> PgtPagamentos => Set<PgtPagamento>();

    public DbSet<GovDenuncia> GovDenuncias => Set<GovDenuncia>();

    public DbSet<PrpProposta> PrpPropostas => Set<PrpProposta>();
    public DbSet<PrpAceite> PrpAceites => Set<PrpAceite>();
    public DbSet<PrpInfo> PrpInfos => Set<PrpInfo>();

    public DbSet<TrnLog> TrnLogs => Set<TrnLog>();
    public DbSet<TrnTipo> TrnTipos => Set<TrnTipo>();

    public DbSet<NtfNotificacao> NtfNotificacoes => Set<NtfNotificacao>();
    public DbSet<NtfTipo> NtfTipos => Set<NtfTipo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
