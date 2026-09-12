using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Folha.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Folha.Api.Services;

public class TokenService(IConfiguration config)
{
    public string GenerateToken(User user)
    {
        var key = config["Jwt:Key"]!;
        var issuer = config["Jwt:Issuer"];
        var audience = config["Jwt:Audience"];

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(config["Jwt:ExpiresInMinutes"]!)),
    signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
