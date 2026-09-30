namespace VassCommerce.Api.Models;

public class Pagamento
{
    public int Id { get; set; }
    public decimal ValorPago { get; set; }

    public int PedidoId { get; set; }
    public int ClienteId { get; set; }
    public int CartaoId { get; set; }
}
