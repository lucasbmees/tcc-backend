using System.Security.Claims;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Application.Abstractions.Security;

public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string passwordHash);
}

public interface IJwtTokenService
{
    string GenerateToken(UsuUsuario usuario);
}

public interface ICurrentUser
{
    long? UserId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    ClaimsPrincipal Principal { get; }
}

