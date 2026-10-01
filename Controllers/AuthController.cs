using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VassCommerce.Api.Data;
using VassCommerce.Api.Dtos;
using VassCommerce.Api.Models;
using VassCommerce.Api.Services;

namespace VassCommerce.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    AppDbContext db,
    AuthService authService,
    JwtTokenService jwtTokenService,
    ClienteService clienteService
) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request
    )
    {
        var usuario = await authService.ValidateAsync(
            AuthService.NormalizeEmail(request.Email),
            request.Senha
        );

        if (usuario is null)
        {
            return InvalidCredentials();
        }

        var cliente = await db.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.UsuarioId == usuario.Id
            );
        var administrador = await db.Administradores
            .AsNoTracking()
            .AnyAsync(
                item => item.UsuarioId == usuario.Id
            );

        var perfil = administrador
            ? "Administrador"
            : cliente is not null
                ? "Cliente"
                : null;

        if (perfil is null)
        {
            return InvalidCredentials();
        }

        return Ok(
            jwtTokenService.CreateToken(
                usuario,
                cliente?.Id ?? 0,
                perfil
            )
        );
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [HttpPost("registro")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request
    )
    {
        var nomeCompleto = request.NomeCompleto.Trim();
        var email = AuthService.NormalizeEmail(request.Email);
        var cpf = request.Cpf.Trim();

        if (nomeCompleto.Length < 2 || string.IsNullOrWhiteSpace(cpf))
        {
            return BadRequest(new
            {
                mensagem =
                    "Nome e CPF devem conter valores válidos."
            });
        }

        if (System.Text.Encoding.UTF8.GetByteCount(request.Senha) > 72)
        {
            return BadRequest(new
            {
                mensagem =
                    "A senha deve ter no máximo 72 bytes em UTF-8."
            });
        }

        if (await db.Usuarios.AnyAsync(
                usuario => usuario.Email.ToLower() == email
            ))
        {
            return Conflict(new
            {
                mensagem = "E-mail já cadastrado."
            });
        }

        if (await db.Clientes.AnyAsync(
                cliente => cliente.Cpf == cpf
            ))
        {
            return Conflict(new
            {
                mensagem = "CPF já cadastrado."
            });
        }

        var agora = DateTime.UtcNow;
        var usuarioNovo = new Usuario
        {
            NomeCompleto = nomeCompleto,
            Email = email,
            Senha = authService.HashPassword(request.Senha),
            FotoUrl = string.Empty,
            DataCadastro = agora,
            DataUltimaAtualizacao = agora
        };
        var clienteNovo = new Cliente
        {
            Usuario = usuarioNovo,
            Cpf = cpf,
            DataNascimento = request.DataNascimento!.Value
        };

        db.Clientes.Add(clienteNovo);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            return Conflict(new
            {
                mensagem = "E-mail ou CPF já cadastrado."
            });
        }

        var resposta = jwtTokenService.CreateToken(
            usuarioNovo,
            clienteNovo.Id,
            "Cliente"
        );

        return StatusCode(
            StatusCodes.Status201Created,
            resposta
        );
    }

    [Authorize(Roles = "Cliente")]
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
        var cliente = await clienteService.GetByUserAsync(userId);

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

    [Authorize]
    [HttpGet("perfil")]
    public IActionResult Perfil()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            ),
            nome = User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(ClaimTypes.Email),
            perfil = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet("area-administrativa")]
    public IActionResult AreaAdministrativa()
    {
        return Ok(new
        {
            mensagem = "Acesso administrativo autorizado."
        });
    }

    private UnauthorizedObjectResult InvalidCredentials()
    {
        return Unauthorized(new
        {
            mensagem = "E-mail ou senha inválidos."
        });
    }

    private static bool IsUniqueConstraintViolation(
        DbUpdateException exception
    )
    {
        return exception.GetBaseException() is SqlException sqlException &&
            sqlException.Number is 2601 or 2627;
    }
}
