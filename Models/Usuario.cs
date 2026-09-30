namespace VassCommerce.Api.Models;

/// <summary>
/// Representa os dados de acesso e identificação de um usuário.
/// Um usuário pode estar associado a um cliente ou administrador.
/// </summary>
public class Usuario
{
    public int Id { get; set; }

    public string NomeCompleto { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Senha { get; set; } = string.Empty;

    public string FotoUrl { get; set; } = string.Empty;

    public DateTime DataCadastro { get; set; }

    public DateTime DataUltimaAtualizacao { get; set; }

    // Um usuário pode estar associado a um cliente.
    public Cliente? Cliente { get; set; }

    public Administrador? Administrador { get; set; }
}