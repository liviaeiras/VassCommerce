namespace VassCommerce.Api.Models;

public class Endereco
{
    public int Id { get; set; }
    public string Rua { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string Cep { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public long Telefone { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
    public DateTime DataUltimaAtualizacao { get; set; }

    // Relacionamento "localizado em": um endereço está em uma cidade.
    public int CidadeId { get; set; }

    // Todo endereço pertence a um cliente (relacionamento 1 - 0..1 no diagrama).
    public int ClienteId { get; set; }
}
