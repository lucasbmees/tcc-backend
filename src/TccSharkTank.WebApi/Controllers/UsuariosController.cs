using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Application.Services;

namespace TccSharkTank.WebApi.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarios;
    private readonly ICurrentUser _currentUser;

    public UsuariosController(IUsuarioService usuarios, ICurrentUser currentUser)
    {
        _usuarios = usuarios;
        _currentUser = currentUser;
    }

    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<UserDetailsResponse> Details([FromRoute] long id, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != id && _currentUser.Role != "adm")
        {
            throw new TccSharkTank.Application.Common.AppException("Acesso negado.", 403);
        }

        return await _usuarios.GetDetailsAsync(id, cancellationToken);
    }

    [Authorize]
    [HttpPut("{id:long}")]
    public async Task<UserDetailsResponse> Update([FromRoute] long id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != id && _currentUser.Role != "adm")
        {
            throw new TccSharkTank.Application.Common.AppException("Acesso negado.", 403);
        }

        return await _usuarios.UpdateAsync(id, request, cancellationToken);
    }
}

[ApiController]
[Route("api/admin/usuarios")]
[Authorize(Roles = "adm")]
public sealed class AdminUsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarios;

    public AdminUsuariosController(IUsuarioService usuarios)
    {
        _usuarios = usuarios;
    }

    [HttpGet]
    public Task<List<UserDetailsResponse>> List(CancellationToken cancellationToken)
        => _usuarios.AdminListAsync(cancellationToken);

    [HttpPatch("{id:long}/status")]
    public Task<UserDetailsResponse> SetStatus([FromRoute] long id, [FromQuery] bool ativo, CancellationToken cancellationToken)
        => _usuarios.AdminSetStatusAsync(id, ativo, cancellationToken);
}

