namespace VassCommerce.Api.Models;

public class Pedido
{
    public int Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public decimal ValorTotal { get; set; }
    public PedidoStatus StatusAtual { get; set; }

    // Relacionamento "realizado por": todo pedido é feito por um cliente.
    public int ClienteId { get; set; }
}
