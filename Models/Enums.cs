namespace VassCommerce.Api.Models;

/// <summary>
/// Status possíveis de um pedido, conforme diagrama de classes.
/// </summary>
public enum PedidoStatus
{
    AGUARDANDO_PAGAMENTO = 1,
    PAGO = 5,
    SEPARANDO_ESTOQUE = 2,
    ENTREGUE_TRANSPORTADORA = 3,
    ENTREGUE_CLIENTE = 4
}

/// <summary>
/// Tipos de cartão aceitos, conforme diagrama de classes.
/// </summary>
public enum TipoCartao
{
    DEBITO = 1,
    CREDITO = 2
}
