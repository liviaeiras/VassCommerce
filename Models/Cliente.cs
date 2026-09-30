namespace VassCommerce.Api.Models;

public class Cliente
{
    public int Id { get; set; }
    public DateTime DataNascimento { get; set; }
    public string Cpf { get; set; } = string.Empty;

    // Todo cliente é um usuário ("titular").
    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
