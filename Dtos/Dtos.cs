namespace VassCommerce.Api.Dtos;

using VassCommerce.Api.Models;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string ImagemSimboloUrl { get; set; } = string.Empty;
}

public class ProdutoResumoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal ValorUnitario { get; set; }
    public decimal? ValorComPromocao { get; set; }
}

public class ProdutoDetalheDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string FotoUrl { get; set; } = string.Empty;
    public decimal ValorUnitario { get; set; }
    public decimal? ValorComPromocao { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime DataUltimaAtualizacao { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
}

public class ClienteDto
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FotoUrl { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string Cpf { get; set; } = string.Empty;
}

public class EnderecoDto
{
    public int Id { get; set; }
    public string Rua { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string Cep { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public long Telefone { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class CartaoDto
{
    public int Id { get; set; }
    public TipoCartao Tipo { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class TipoCartaoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class EstadoDto
{
    public int Id { get; set; }
    public string Sigla { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
}

public class CidadeDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class ItemPedidoDto
{
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class PedidoDto
{
    public int Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public decimal ValorTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ItemPedidoDto> Itens { get; set; } = new();
}

public class PagamentoDto
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public decimal ValorPago { get; set; }
}
