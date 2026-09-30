using Microsoft.EntityFrameworkCore;
using VassCommerce.Api.Data;
using VassCommerce.Api.Models;

namespace VassCommerce.Api.Services;

public class AuthService(AppDbContext db)
{
    public async Task<Usuario?> ValidateAsync(
        string email,
        string senha
    )
    {
        var emailNormalizado = NormalizeEmail(email);
        var usuario = await db.Usuarios.FirstOrDefaultAsync(
            item => item.Email.ToLower() == emailNormalizado
        );

        if (usuario is null)
        {
            return null;
        }

        return BCrypt.Net.BCrypt.Verify(senha, usuario.Senha)
            ? usuario
            : null;
    }

    public string HashPassword(string senha)
    {
        return BCrypt.Net.BCrypt.HashPassword(senha);
    }

    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
