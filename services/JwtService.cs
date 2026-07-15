using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Parkini.Entities;

namespace Parkini.Infrastructure.Security;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public (string token, DateTime expireLe) GenererToken(Utilisateur utilisateur)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, utilisateur.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, utilisateur.Email),
            new(ClaimTypes.Role, utilisateur.Role?.Nom ?? "Utilisateur"),
            new("prenom", utilisateur.Prenom),
            new("nom", utilisateur.Nom),
        };

        var cle = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Cle"]!));
        var credentials = new SigningCredentials(cle, SecurityAlgorithms.HmacSha256);
        var expireLe = DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:DureeValiditeHeures"] ?? "8"));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Emetteur"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expireLe,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expireLe);
    }
}