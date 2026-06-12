using Microsoft.AspNetCore.Mvc;
using TccSharkTank.Application.Contracts;
using TccSharkTank.Application.Services;

namespace TccSharkTank.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public Task<AuthResponse> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
        => _auth.RegisterAsync(request, cancellationToken);

    [HttpPost("login")]
    public Task<AuthResponse> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => _auth.LoginAsync(request, cancellationToken);

    [HttpPost("recuperar-senha")]
    public Task<RecuperarSenhaResponse> RecuperarSenha([FromBody] RecuperarSenhaRequest request, CancellationToken cancellationToken)
        => _auth.RecuperarSenhaAsync(request, cancellationToken);

    [HttpPost("redefinir-senha")]
    public Task<MensagemResponseSimples> RedefinirSenha([FromBody] RedefinirSenhaRequest request, CancellationToken cancellationToken)
        => _auth.RedefinirSenhaAsync(request, cancellationToken);
}
