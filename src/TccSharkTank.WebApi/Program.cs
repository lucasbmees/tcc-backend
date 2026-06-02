using TccSharkTank.Application.Services;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Infrastructure;
using TccSharkTank.Domain.Entities;
using TccSharkTank.WebApi.Middleware;
using TccSharkTank.WebApi.Security;
// Adicione o namespace do seu DbContext se necessário (ex: using TccSharkTank.Infrastructure.Persistence;)

static void LoadDotEnv()
{
    static string? FindDotEnv(string start)
    {
        var current = start;
        for (var i = 0; i < 8; i++)
        {
            var candidate = Path.Combine(current, ".env");
            if (File.Exists(candidate)) return candidate;
            var parent = Directory.GetParent(current);
            if (parent is null) return null;
            current = parent.FullName;
        }
        return null;
    }

    var path = FindDotEnv(Directory.GetCurrentDirectory());
    if (path is null) return;

    foreach (var rawLine in File.ReadAllLines(path))
    {
        var line = rawLine.Trim();
        if (line.Length == 0) continue;
        if (line.StartsWith('#')) continue;
        var idx = line.IndexOf('=');
        if (idx <= 0) continue;
        var key = line[..idx].Trim();
        if (key.Length == 0) continue;
        var value = line[(idx + 1)..].Trim();
        if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
        {
            value = value[1..^1];
        }
        if (Environment.GetEnvironmentVariable(key) is null)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

// Configurações de Serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TccSharkTank API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Injeção de Dependência da Infraestrutura e Segurança
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

// Configuração de Autenticação JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = jwtSection["Key"] ?? "dev-secret-change-me-please-dev-secret-change-me-please";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// Adicione isto antes do builder.Build()
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

var app = builder.Build();

// ==========================================
// BLOCO PARA FORÇAR CRIAÇÃO DAS TABELAS
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Substitua 'AppDbContext' pelo nome exato da sua classe de contexto se for diferente
        var context = services.GetRequiredService<TccSharkTank.Infrastructure.Persistence.AppDbContext>();
        context.Database.Migrate();

        if (app.Environment.IsDevelopment())
        {
            var hasher = services.GetRequiredService<IPasswordHasher>();
            var now = DateTime.UtcNow;

            var cargoInvestidor = context.UsuCargos.FirstOrDefault(c => c.Nome == "investidor");
            if (cargoInvestidor is null)
            {
                cargoInvestidor = new UsuCargo { Nome = "investidor" };
                context.UsuCargos.Add(cargoInvestidor);
                context.SaveChanges();
            }

            var cargoEmpreendedor = context.UsuCargos.FirstOrDefault(c => c.Nome == "empreendedor");
            if (cargoEmpreendedor is null)
            {
                cargoEmpreendedor = new UsuCargo { Nome = "empreendedor" };
                context.UsuCargos.Add(cargoEmpreendedor);
                context.SaveChanges();
            }

            var cargoAdm = context.UsuCargos.FirstOrDefault(c => c.Nome == "adm");
            if (cargoAdm is null)
            {
                cargoAdm = new UsuCargo { Nome = "adm" };
                context.UsuCargos.Add(cargoAdm);
                context.SaveChanges();
            }

            var planoBasico = context.UsuPlanos.FirstOrDefault(p => p.Nome == "basico");
            if (planoBasico is null)
            {
                planoBasico = new UsuPlano { Nome = "basico" };
                context.UsuPlanos.Add(planoBasico);
                context.SaveChanges();
            }

            var planoElite = context.UsuPlanos.FirstOrDefault(p => p.Nome == "elite");
            if (planoElite is null)
            {
                planoElite = new UsuPlano { Nome = "elite" };
                context.UsuPlanos.Add(planoElite);
                context.SaveChanges();
            }

            var planoPro = context.UsuPlanos.FirstOrDefault(p => p.Nome == "pro");
            if (planoPro is null)
            {
                planoPro = new UsuPlano { Nome = "pro" };
                context.UsuPlanos.Add(planoPro);
                context.SaveChanges();
            }

            UsuUsuario UpsertUser(string email, string senha, string cpf, string telefone, string nome, string sobrenome, int cargoId, int planoId)
            {
                var usuario = context.UsuUsuarios.FirstOrDefault(u => u.Email == email);
                if (usuario is null)
                {
                    usuario = new UsuUsuario
                    {
                        Cpf = cpf,
                        Email = email,
                        Telefone = telefone,
                        Senha = hasher.Hash(senha),
                        CargoId = cargoId,
                        PlanoId = planoId,
                        Status = true,
                        UltimoLogin = null,
                        Nome = nome,
                        Sobrenome = sobrenome
                    };
                    context.UsuUsuarios.Add(usuario);
                }
                else
                {
                    usuario.Senha = hasher.Hash(senha);
                    usuario.CargoId = cargoId;
                    usuario.PlanoId = planoId;
                    usuario.Status = true;
                    usuario.Nome = nome;
                    usuario.Sobrenome = sobrenome;
                    context.UsuUsuarios.Update(usuario);
                }

                context.SaveChanges();
                return usuario;
            }

            var investidorElite = UpsertUser(
                email: "elite@tcc.local",
                senha: "123456",
                cpf: "99999999990",
                telefone: "11999999990",
                nome: "Investidor",
                sobrenome: "Elite",
                cargoId: cargoInvestidor.Id,
                planoId: planoElite.Id);

            var empreendedorPro = UpsertUser(
                email: "pro@tcc.local",
                senha: "123456",
                cpf: "99999999991",
                telefone: "11999999991",
                nome: "Empreendedor",
                sobrenome: "Pro",
                cargoId: cargoEmpreendedor.Id,
                planoId: planoPro.Id);

            var admin = UpsertUser(
                email: "admin@tcc.local",
                senha: "123456",
                cpf: "99999999992",
                telefone: "11999999992",
                nome: "Admin",
                sobrenome: "Sistema",
                cargoId: cargoAdm.Id,
                planoId: planoBasico.Id);

            var investidorBasico = UpsertUser(
                email: "investidor@tcc.local",
                senha: "123456",
                cpf: "99999999993",
                telefone: "11999999993",
                nome: "Investidor",
                sobrenome: "Basico",
                cargoId: cargoInvestidor.Id,
                planoId: planoBasico.Id);

            var empreendedorBasico = UpsertUser(
                email: "empreendedor@tcc.local",
                senha: "123456",
                cpf: "99999999994",
                telefone: "11999999994",
                nome: "Empreendedor",
                sobrenome: "Basico",
                cargoId: cargoEmpreendedor.Id,
                planoId: planoBasico.Id);

            var ideia = context.IdaIdeias
                .Include(i => i.Info)
                .Include(i => i.Documentos)
                .FirstOrDefault(i => i.UsuarioId == empreendedorPro.Id && i.Nome == "Ideia Demo (Docs + Proposta Aceita)");

            if (ideia is null)
            {
                ideia = new IdaIdeia
                {
                    Id = 0,
                    UsuarioId = empreendedorPro.Id,
                    StatusId = 1,
                    MotivoStatus = null,
                    CategoriaId = 1,
                    EstagioId = 2,
                    Nome = "Ideia Demo (Docs + Proposta Aceita)",
                    Regiao = "São Paulo - SP",
                    CreateDate = now,
                    UpdateDate = now,
                    Info = new IdaInfo
                    {
                        Id = 0,
                        Cnpj = "00.000.000/0001-91",
                        Descricao = "Ideia demo para testar recursos pagos (Elite/Pro).",
                        LinkVideo = null,
                        Imagem = null,
                        Fatia = 10m,
                        ValorCaptacao = 100000m,
                        CreateDate = now,
                        UpdateDate = now
                    }
                };

                context.IdaIdeias.Add(ideia);
                context.SaveChanges();

                context.IdaDocumentos.Add(new IdaDocumento
                {
                    IdeiaId = ideia.Id,
                    Arquivo = "docs/demo-elite.pdf"
                });

                context.SaveChanges();
            }

            var proposta = context.PrpPropostas
                .Include(p => p.Infos)
                .FirstOrDefault(p => p.IdeiaId == ideia.Id && p.UsuarioId == investidorElite.Id);

            if (proposta is null)
            {
                proposta = new PrpProposta
                {
                    Id = 0,
                    IdeiaId = ideia.Id,
                    UsuarioId = investidorElite.Id,
                    Status = true,
                    CreateDate = now,
                    UpdateDate = now
                };
                context.PrpPropostas.Add(proposta);
                context.SaveChanges();
            }

            var ultima = proposta.Infos.OrderByDescending(i => i.CreateDate).FirstOrDefault();
            if (ultima?.AceiteId != 1)
            {
                context.PrpInfos.Add(new PrpInfo
                {
                    Id = 0,
                    PropostaId = proposta.Id,
                    Mensagem = "Proposta demo (aceita) para liberar chat/contrato.",
                    Valor = 50000m,
                    FatiaPret = 5m,
                    AceiteId = 1,
                    Retorno = "Aceito (demo).",
                    CreateDate = now,
                    UpdateDate = now
                });
                proposta.UpdateDate = now;
                context.PrpPropostas.Update(proposta);
                context.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao criar as tabelas do banco de dados.");
    }
}
// ==========================================

// Pipeline de Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger habilitado para todos os ambientes
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TccSharkTank API v1");
    c.RoutePrefix = ""; 
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
