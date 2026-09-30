namespace VassCommerce.Api.Models;

public class Categoria
{
    public int Id { get; set; }
    public string ImagemSimboloUrl { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    public ICollection<Produto> Produtos { get; set; }
        = new List<Produto>();

    public Administrador? Administrador { get; set; }   
}
