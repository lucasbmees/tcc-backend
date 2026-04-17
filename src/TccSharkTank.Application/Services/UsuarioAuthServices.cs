using TccSharkTank.Application.Abstractions.Persistence;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Application.Abstractions.System;
using TccSharkTank.Application.Common;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}

public interface IUsuarioService
{
    Task<UserDetailsResponse> GetDetailsAsync(long usuId, CancellationToken cancellationToken);
    Task<UserDetailsResponse> UpdateAsync(long usuId, UpdateUserRequest request, CancellationToken cancellationToken);
    Task<List<UserDetailsResponse>> AdminListAsync(CancellationToken cancellationToken);
    Task<UserDetailsResponse> AdminSetStatusAsync(long usuId, bool ativo, CancellationToken cancellationToken);
}

public sealed class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly ICargoRepository _cargos;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IClock _clock;
    private readonly ILogService _logs;

    public AuthService(
        IUsuarioRepository usuarios,
        ICargoRepository cargos,
        IUnitOfWork uow,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        IClock clock,
        ILogService logs)
    {
        _usuarios = usuarios;
        _cargos = cargos;
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _clock = clock;
        _logs = logs;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var cpf = request.Cpf.Trim();
        var telefone = request.Telefone.Trim();

        if (await _usuarios.GetByEmailAsync(email, cancellationToken) is not null)
        {
            throw new AppException("E-mail já cadastrado.", 409);
        }

        if (await _usuarios.GetByCpfAsync(cpf, cancellationToken) is not null)
        {
            throw new AppException("CPF já cadastrado.", 409);
        }

        if (await _usuarios.GetByTelefoneAsync(telefone, cancellationToken) is not null)
        {
            throw new AppException("Telefone já cadastrado.", 409);
        }

        var cargo = await _cargos.GetByNomeAsync(request.CargoNome.Trim().ToLowerInvariant(), cancellationToken);
        if (cargo is null)
        {
            throw new AppException("Cargo inválido.", 400);
        }

        var usuario = new UsuUsuario
        {
            Cpf = cpf,
            Email = email,
            Telefone = telefone,
            Senha = _passwordHasher.Hash(request.Senha),
            CargoId = cargo.Id,
            Status = true,
            UltimoLogin = null,
            Nome = request.Nome.Trim(),
            Sobrenome = request.Sobrenome.Trim()
        };

        await _usuarios.AddAsync(usuario, cancellationToken);
        await _logs.RegistrarAsync(tipoNome: "cadastro", usuarioId: null, ideiaId: null, propostaId: null, descricao: $"Cadastro de usuário {usuario.Email}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var token = _jwt.GenerateToken(usuario);
        return new AuthResponse(usuario.Id, cargo.Nome, token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (usuario is null || !usuario.Status)
        {
            throw new AppException("Credenciais inválidas.", 401);
        }

        if (!_passwordHasher.Verify(request.Senha, usuario.Senha))
        {
            throw new AppException("Credenciais inválidas.", 401);
        }

        usuario.UltimoLogin = _clock.UtcNow;
        _usuarios.Update(usuario);

        await _logs.RegistrarAsync(tipoNome: "login", usuarioId: usuario.Id, ideiaId: null, propostaId: null, descricao: $"Login do usuário {usuario.Email}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var token = _jwt.GenerateToken(usuario);
        return new AuthResponse(usuario.Id, usuario.Cargo?.Nome ?? usuario.CargoId.ToString(), token);
    }
}

public sealed class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly ILogService _logs;

    public UsuarioService(IUsuarioRepository usuarios, IUnitOfWork uow, IClock clock, ILogService logs)
    {
        _usuarios = usuarios;
        _uow = uow;
        _clock = clock;
        _logs = logs;
    }

    public async Task<UserDetailsResponse> GetDetailsAsync(long usuId, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.GetByIdAsync(usuId, cancellationToken);
        if (usuario is null)
        {
            throw new AppException("Usuário não encontrado.", 404);
        }

        return MapDetails(usuario);
    }

    public async Task<UserDetailsResponse> UpdateAsync(long usuId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.GetByIdAsync(usuId, cancellationToken);
        if (usuario is null)
        {
            throw new AppException("Usuário não encontrado.", 404);
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            usuario.Email = request.Email.Trim().ToLowerInvariant();
        }
        if (!string.IsNullOrWhiteSpace(request.Telefone))
        {
            usuario.Telefone = request.Telefone.Trim();
        }
        if (!string.IsNullOrWhiteSpace(request.Nome))
        {
            usuario.Nome = request.Nome.Trim();
        }
        if (!string.IsNullOrWhiteSpace(request.Sobrenome))
        {
            usuario.Sobrenome = request.Sobrenome.Trim();
        }
        if (request.Inativar is true)
        {
            usuario.Status = false;
        }

        if (request.Perfil is not null)
        {
            usuario.Perfil ??= new UsuPerfil
            {
                Id = 0,
                UsuarioId = usuario.Id,
                CreateDate = _clock.UtcNow,
                UpdateDate = _clock.UtcNow,
            };

            usuario.Perfil.Descricao = request.Perfil.Descricao;
            usuario.Perfil.Cep = request.Perfil.Cep;
            usuario.Perfil.DataNasc = request.Perfil.DataNasc;
            usuario.Perfil.LinkRedes = request.Perfil.LinkRedes;
            usuario.Perfil.UpdateDate = _clock.UtcNow;
        }

        _usuarios.Update(usuario);
        await _logs.RegistrarAsync(tipoNome: "edição", usuarioId: usuario.Id, ideiaId: null, propostaId: null, descricao: $"Edição de usuário {usuario.Email}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return MapDetails(usuario);
    }

    public async Task<List<UserDetailsResponse>> AdminListAsync(CancellationToken cancellationToken)
    {
        var usuarios = await _usuarios.ListAsync(cancellationToken);
        return usuarios.Select(MapDetails).ToList();
    }

    public async Task<UserDetailsResponse> AdminSetStatusAsync(long usuId, bool ativo, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.GetByIdAsync(usuId, cancellationToken);
        if (usuario is null)
        {
            throw new AppException("Usuário não encontrado.", 404);
        }

        usuario.Status = ativo;
        _usuarios.Update(usuario);
        await _logs.RegistrarAsync(tipoNome: "edição", usuarioId: usuario.Id, ideiaId: null, propostaId: null, descricao: $"Admin alterou status do usuário {usuario.Email} para {(ativo ? "ativo" : "inativo")}", cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return MapDetails(usuario);
    }

    private static UserDetailsResponse MapDetails(UsuUsuario u)
    {
        return new UserDetailsResponse(
            UsuId: u.Id,
            UsuCpf: u.Cpf,
            UsuEmail: u.Email,
            UsuTelefone: u.Telefone,
            UsuStatus: u.Status,
            UsuUltimoLogin: u.UltimoLogin,
            UsuNome: u.Nome,
            UsuSobrenome: u.Sobrenome,
            Cargo: u.Cargo?.Nome ?? u.CargoId.ToString(),
            Perfil: u.Perfil is null
                ? null
                : new PerfilResponse(
                    Descricao: u.Perfil.Descricao,
                    Cep: u.Perfil.Cep,
                    DataNasc: u.Perfil.DataNasc,
                    LinkRedes: u.Perfil.LinkRedes,
                    CreateDate: u.Perfil.CreateDate,
                    UpdateDate: u.Perfil.UpdateDate)
        );
    }
}
