using System.ComponentModel.DataAnnotations;

namespace VassCommerce.Api.Dtos;

public sealed class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Senha { get; set; } = string.Empty;
}

public sealed class RegisterRequest
{
    [Required, MinLength(2)] public string NomeCompleto { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Senha { get; set; } = string.Empty;
    [Required] public DateTime DataNascimento { get; set; }
    [Required] public string Cpf { get; set; } = string.Empty;
}

public sealed class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
}
