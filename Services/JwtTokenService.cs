using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VassCommerce.Api.Dtos;
using VassCommerce.Api.Models;

namespace VassCommerce.Api.Services;

public sealed class JwtTokenService(IConfiguration configuration)
{
    private readonly IConfigurationSection _jwt =
        configuration.GetSection("Jwt");

    public AuthResponse CreateToken(
        Usuario usuario,
        int clienteId,
        string perfil
    )
    {
        var chave = _jwt["Key"]
            ?? throw new InvalidOperationException(
                "A configuração Jwt:Key não foi encontrada."
            );
        var emissor = _jwt["Issuer"]
            ?? throw new InvalidOperationException(
                "A configuração Jwt:Issuer não foi encontrada."
            );
        var audiencia = _jwt["Audience"]
            ?? throw new InvalidOperationException(
                "A configuração Jwt:Audience não foi encontrada."
            );
        var minutos = _jwt.GetValue<int>("ExpiresMinutes");
        var expiraEm = DateTime.UtcNow.AddMinutes(minutos);

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                usuario.Id.ToString()
            ),
            new Claim(
                ClaimTypes.NameIdentifier,
                usuario.Id.ToString()
            ),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.NomeCompleto),
            new Claim(ClaimTypes.Role, perfil)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
            SecurityAlgorithms.HmacSha256
        );
        var token = new JwtSecurityToken(
            issuer: emissor,
            audience: audiencia,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credentials
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UsuarioId = usuario.Id,
            ClienteId = clienteId,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            Perfil = perfil,
            ExpiraEm = expiraEm
        };
    }
}
