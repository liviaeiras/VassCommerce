using System.ComponentModel.DataAnnotations;

namespace VassCommerce.Api.Dtos;

public sealed class LoginRequest
{
    private string _email = string.Empty;

    [Required, EmailAddress]
    public string Email
    {
        get => _email;
        set => _email = value?.Trim() ?? string.Empty;
    }

    [Required]
    public string Senha { get; set; } = string.Empty;
}

public sealed class RegisterRequest
{
    private string _email = string.Empty;

    [Required, MinLength(2), StringLength(120)]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email
    {
        get => _email;
        set => _email = value?.Trim() ?? string.Empty;
    }

    [Required, MinLength(8), StringLength(128)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public DateTime? DataNascimento { get; set; }

    [Required]
    public string Cpf { get; set; } = string.Empty;
}

public sealed class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public int ClienteId { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
}
