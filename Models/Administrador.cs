namespace VassCommerce.Api.Models;

public class Administrador
{
    public int Id { get; set; }

    // Chave estrangeira do usuário
    public int UsuarioId { get; set; }

    // Navegação para o usuário
    public Usuario Usuario { get; set; } = null!;

    // Chave estrangeira da categoria
    public int CategoriaId { get; set; }

    // Navegação para a categoria
    public Categoria Categoria { get; set; } = null!;
}