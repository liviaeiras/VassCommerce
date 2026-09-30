using System.ComponentModel.DataAnnotations;
using VassCommerce.Api.Models;

namespace VassCommerce.Api.Dtos;

public class CategoriaRequest
{
    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [MinLength(
        2,
        ErrorMessage = "O nome deve possuir pelo menos 2 caracteres."
    )]
    [MaxLength(
        120,
        ErrorMessage = "O nome deve possuir no máximo 120 caracteres."
    )]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(
        500,
        ErrorMessage = "A descrição deve possuir no máximo 500 caracteres."
    )]
    public string Descricao { get; set; } = string.Empty;

    public string ImagemSimboloUrl { get; set; } = string.Empty;
}

public class ProdutoRequest
{
    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [MinLength(
        2,
        ErrorMessage = "O nome deve possuir pelo menos 2 caracteres."
    )]
    [MaxLength(
        180,
        ErrorMessage = "O nome deve possuir no máximo 180 caracteres."
    )]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(
        2000,
        ErrorMessage = "A descrição deve possuir no máximo 2000 caracteres."
    )]
    public string Descricao { get; set; } = string.Empty;

    public string FotoUrl { get; set; } = string.Empty;

    [Range(
        typeof(decimal),
        "0.01",
        "999999999.99",
        ErrorMessage = "O valor deve ser maior ou igual a 0,01.",
        ParseLimitsInInvariantCulture = true
    )]
    public decimal ValorUnitario { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Informe uma categoria válida."
    )]
    public int CategoriaId { get; set; }
}

public class EnderecoRequest
{
    [Required(ErrorMessage = "A rua é obrigatória.")]
    public string Rua { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Informe um número válido."
    )]
    public int Numero { get; set; }

    [Required(ErrorMessage = "O CEP é obrigatório.")]
    public string Cep { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    public long Telefone { get; set; }

    [Required(ErrorMessage = "O bairro é obrigatório.")]
    public string Bairro { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Informe uma cidade válida."
    )]
    public int CidadeId { get; set; }
}

public class CartaoRequest
{
    public TipoCartao Tipo { get; set; }
}

public class PedidoRequest
{
    [Required(ErrorMessage = "A lista de itens é obrigatória.")]
    [MinLength(
        1,
        ErrorMessage = "O pedido deve possuir pelo menos um item."
    )]
    public List<ItemPedidoRequest> Itens { get; set; } = new();
}

public class ItemPedidoRequest
{
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Informe um produto válido."
    )]
    public int ProdutoId { get; set; }

    [Range(
        1,
        10000,
        ErrorMessage = "A quantidade deve estar entre 1 e 10.000."
    )]
    public int Quantidade { get; set; }
}