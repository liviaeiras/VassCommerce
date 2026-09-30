namespace VassCommerce.Api.Models;

public class Cartao
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool Excluido { get; set; }
    public TipoCartao Tipo { get; set; }

    // Um cartão tem como titular um cliente.
    public int ClienteId { get; set; }
}
