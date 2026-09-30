using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using VassCommerce.Api.Data;
using VassCommerce.Api.Dtos;
using VassCommerce.Api.Models;
using VassCommerce.Api.Services;

namespace VassCommerce.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    AppDbContext db,
    IConfiguration configuration,
    AuthService authService,
    ClienteService clienteService
) : ControllerBase
{
    // Login de cliente ou administrador
    [HttpPost("login")]
    public ActionResult<AuthResponse> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = authService.Validate(
            email,
            request.Senha
        );

        if (user is null)
        {
            return Unauthorized(new
            {
                mensagem = "E-mail ou senha inválidos."
            });
        }

        var cliente = db.Clientes
            .FirstOrDefault(x => x.UsuarioId == user.Id);

        var administrador = db.Administradores
            .FirstOrDefault(x => x.UsuarioId == user.Id);

        string perfil;

        if (administrador is not null)
        {
            perfil = "Administrador";
        }
        else if (cliente is not null)
        {
            perfil = "Cliente";
        }
        else
        {
            return Unauthorized(new
            {
                mensagem = "O usuário não possui um perfil válido."
            });
        }

        var resposta = CreateToken(
            user,
            cliente?.Id ?? 0,
            perfil
        );

        return Ok(resposta);
    }

    // Cadastro público de cliente
    [HttpPost("register")]
    [HttpPost("registro")]
    public ActionResult<AuthResponse> Register(
        RegisterRequest request
    )
    {
        var email = request.Email
            .Trim()
            .ToLowerInvariant();

        var cpf = request.Cpf.Trim();

        if (db.Usuarios.Any(x => x.Email == email))
        {
            return Conflict(new
            {
                mensagem = "E-mail já cadastrado."
            });
        }

        if (db.Clientes.Any(x => x.Cpf == cpf))
        {
            return Conflict(new
            {
                mensagem = "CPF já cadastrado."
            });
        }

        using var transaction =
            db.Database.BeginTransaction();

        try
        {
            var now = DateTime.UtcNow;

            var user = new Usuario
            {
                NomeCompleto = request.NomeCompleto.Trim(),
                Email = email,
                Senha = BCrypt.Net.BCrypt.HashPassword(
                    request.Senha
                ),
                FotoUrl = string.Empty,
                DataCadastro = now,
                DataUltimaAtualizacao = now
            };

            db.Usuarios.Add(user);
            db.SaveChanges();

            var cliente = new Cliente
            {
                UsuarioId = user.Id,
                Cpf = cpf,
                DataNascimento = request.DataNascimento
            };

            db.Clientes.Add(cliente);
            db.SaveChanges();

            // Todo cadastro público gera um cliente.
            var resposta = CreateToken(
                user,
                cliente.Id,
                "Cliente"
            );

            transaction.Commit();

            return StatusCode(
                StatusCodes.Status201Created,
                resposta
            );
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // Retorna os dados do cliente autenticado
    [Authorize]
    [HttpGet("/cliente/me")]
    public async Task<IActionResult> Me()
    {
        var claimId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (!int.TryParse(claimId, out var userId))
        {
            return Unauthorized(new
            {
                mensagem = "Token inválido."
            });
        }

        var user = await clienteService.UserAsync(userId);

        var cliente =
            await clienteService.GetByUserAsync(userId);

        if (user is null || cliente is null)
        {
            return NotFound(new
            {
                mensagem = "Cliente não encontrado."
            });
        }

        var resultado = new ClienteDto
        {
            Id = cliente.Id,
            NomeCompleto = user.NomeCompleto,
            Email = user.Email,
            FotoUrl = user.FotoUrl,
            DataNascimento = cliente.DataNascimento,
            Cpf = cliente.Cpf
        };

        return Ok(resultado);
    }

    // Endpoint temporário para visualizar as claims do token
    [Authorize]
    [HttpGet("perfil")]
    public IActionResult Perfil()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        var nome = User.FindFirstValue(
            ClaimTypes.Name
        );

        var email = User.FindFirstValue(
            ClaimTypes.Email
        );

        var perfil = User.FindFirstValue(
            ClaimTypes.Role
        );

        return Ok(new
        {
            userId,
            nome,
            email,
            perfil
        });
    }

    // Endpoint temporário exclusivo para administradores
    [Authorize(Roles = "Administrador")]
    [HttpGet("area-administrativa")]
    public IActionResult AreaAdministrativa()
    {
        return Ok(new
        {
            mensagem =
                "Acesso administrativo autorizado."
        });
    }

    // Criação do token JWT
    private AuthResponse CreateToken(
        Usuario user,
        int clienteId,
        string perfil
    )
    {
        var section =
            configuration.GetSection("Jwt");

        var key = section["Key"]
            ?? throw new InvalidOperationException(
                "A chave JWT não foi configurada."
            );

        var issuer = section["Issuer"]
            ?? throw new InvalidOperationException(
                "O emissor JWT não foi configurado."
            );

        var audience = section["Audience"]
            ?? throw new InvalidOperationException(
                "A audiência JWT não foi configurada."
            );

        var expires = DateTime.UtcNow.AddMinutes(
            section.GetValue<int>(
                "ExpiresMinutes",
                120
            )
        );

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()
            ),

            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),

            new Claim(
                ClaimTypes.Email,
                user.Email
            ),

            new Claim(
                ClaimTypes.Name,
                user.NomeCompleto
            ),

            new Claim(
                ClaimTypes.Role,
                perfil
            )
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key)
        );

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler()
                .WriteToken(token),

            ClienteId = clienteId,
            NomeCompleto = user.NomeCompleto,
            ExpiraEm = expires
        };
    }
}