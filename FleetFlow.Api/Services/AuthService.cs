using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FleetFlow.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace FleetFlow.Api.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, string issuer, string audience, string secret, int expirationMinutes);
    string GenerateRefreshToken();
}

public class TokenService : ITokenService
{
    public string GenerateAccessToken(User user, string issuer, string audience, string secret, int expirationMinutes)
    {
        var key = Encoding.ASCII.GetBytes(secret);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("companyId", user.CompanyId.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }
}

