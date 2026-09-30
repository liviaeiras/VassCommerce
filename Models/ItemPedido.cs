namespace VassCommerce.Api.Models;

/// <summary>
/// Associativa entre Pedido e Produto ("composto por" / "vinculado a").
/// </summary>
public class ItemPedido
{
    public int Id { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }

    public int PedidoId { get; set; }
    public int ProdutoId { get; set; }
}
