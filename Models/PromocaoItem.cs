namespace VassCommerce.Api.Models;

/// <summary>
/// Associativa entre Produto e TabelaPromocao: define o novo preço ("desconto")
/// de um produto específico dentro de uma tabela de promoção.
/// </summary>
public class PromocaoItem
{
    public int Id { get; set; }
    public decimal ValorPromocao { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime DataUltimaAtualizacao { get; set; }

    // Relacionamento "desconto": item de promoção aplicado a um produto.
    public int ProdutoId { get; set; }

    // Relacionamento "pertence a": item pertence a uma tabela de promoção.
    public int TabelaPromocaoId { get; set; }
}
