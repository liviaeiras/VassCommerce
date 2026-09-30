namespace VassCommerce.Api.Models;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string FotoUrl { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
    public DateTime DataUltimaAtualizacao { get; set; }
    public decimal ValorUnitario { get; set; }

    // Relacionamento "do tipo": todo produto pertence a uma categoria.
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
}
