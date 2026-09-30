namespace VassCommerce.Api.Models;

public class Cidade
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    // Relacionamento "localizado em": uma cidade pertence a um estado.
    public int EstadoId { get; set; }
}
