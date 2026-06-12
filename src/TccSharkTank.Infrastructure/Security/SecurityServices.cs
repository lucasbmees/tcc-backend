using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TccSharkTank.Application.Abstractions.Security;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string Hash(string plainTextPassword)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            password: plainTextPassword,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: KeySize);

        return $"pbkdf2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public bool Verify(string plainTextPassword, string passwordHash)
    {
        var parts = passwordHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || parts[0] != "pbkdf2")
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expectedKey = Convert.FromBase64String(parts[3]);

        var actualKey = Rfc2898DeriveBytes.Pbkdf2(
            password: plainTextPassword,
            salt: salt,
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: expectedKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(UsuUsuario usuario)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? "tcc-sharktank";
        var audience = jwtSection["Audience"] ?? "tcc-sharktank";
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado.");
        var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var minutes) ? minutes : 240;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var role = usuario.Cargo?.Nome ?? usuario.CargoId.ToString();
        var plan = usuario.Plano?.Nome ?? usuario.PlanoId.ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, role),
            new("plan", plan),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
