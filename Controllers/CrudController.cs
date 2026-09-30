using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VassCommerce.Api.Data;
using VassCommerce.Api.Dtos;
using VassCommerce.Api.Models;
using VassCommerce.Api.Services;

namespace VassCommerce.Api.Controllers;

[ApiController]
[Route("cliente/me/endereco")]
[Authorize(Roles = "Cliente")]
public class EnderecoCrudController(
    EnderecoService enderecoService,
    ClienteService clienteService,
    AppDbContext db
) : ControllerBase
{
    private async Task<int?> ObterClienteIdAsync()
    {
        var claim = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (!int.TryParse(claim, out var usuarioId))
        {
            return null;
        }

        var cliente = await clienteService.GetByUserAsync(
            usuarioId
        );

        return cliente?.Id;
    }

    private async Task<EnderecoDto> ParaDtoAsync(
        Endereco endereco
    )
    {
        var localizacao = await db.Cidades
            .AsNoTracking()
            .Where(cidade =>
                cidade.Id == endereco.CidadeId
            )
            .Join(
                db.Estados.AsNoTracking(),
                cidade => cidade.EstadoId,
                estado => estado.Id,
                (cidade, estado) => new
                {
                    Cidade = cidade.Nome,
                    Estado = estado.Nome
                }
            )
            .FirstOrDefaultAsync();

        return new EnderecoDto
        {
            Id = endereco.Id,
            Rua = endereco.Rua,
            Numero = endereco.Numero,
            Cep = endereco.Cep,
            Complemento = endereco.Complemento,
            Telefone = endereco.Telefone,
            Bairro = endereco.Bairro,
            Cidade = localizacao?.Cidade ?? string.Empty,
            Estado = localizacao?.Estado ?? string.Empty
        };
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IEnumerable<EnderecoDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    public async Task<IActionResult> Get()
    {
        var clienteId = await ObterClienteIdAsync();

        if (clienteId is null)
        {
            return Unauthorized(new
            {
                mensagem = "Cliente não identificado."
            });
        }

        var enderecos = await enderecoService
            .ForClienteAsync(clienteId.Value);

        var resultado = new List<EnderecoDto>();

        foreach (var endereco in enderecos)
        {
            resultado.Add(
                await ParaDtoAsync(endereco)
            );
        }

        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(EnderecoDto),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> GetById(int id)
    {
        var clienteId = await ObterClienteIdAsync();

        if (clienteId is null)
        {
            return Unauthorized(new
            {
                mensagem = "Cliente não identificado."
            });
        }

        var endereco = await enderecoService.GetOwnedAsync(
            id,
            clienteId.Value
        );

        if (endereco is null)
        {
            return NotFound(new
            {
                mensagem = $"Endereço {id} não encontrado."
            });
        }

        return Ok(
            await ParaDtoAsync(endereco)
        );
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(EnderecoDto),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    public async Task<IActionResult> Post(
        EnderecoRequest request
    )
    {
        var clienteId = await ObterClienteIdAsync();

        if (clienteId is null)
        {
            return Unauthorized(new
            {
                mensagem = "Cliente não identificado."
            });
        }

        var cidadeExiste = await db.Cidades
            .AsNoTracking()
            .AnyAsync(cidade =>
                cidade.Id == request.CidadeId
            );

        if (!cidadeExiste)
        {
            return BadRequest(new
            {
                mensagem =
                    $"A cidade {request.CidadeId} não existe."
            });
        }

        var agora = DateTime.UtcNow;

        var endereco = new Endereco
        {
            ClienteId = clienteId.Value,
            Rua = request.Rua.Trim(),
            Numero = request.Numero,
            Cep = request.Cep.Trim(),
            Complemento = request.Complemento?.Trim(),
            Telefone = request.Telefone,
            Bairro = request.Bairro.Trim(),
            CidadeId = request.CidadeId,
            DataCadastro = agora,
            DataUltimaAtualizacao = agora
        };

        await enderecoService.SaveAsync(endereco);

        return CreatedAtAction(
            nameof(GetById),
            new { id = endereco.Id },
            await ParaDtoAsync(endereco)
        );
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> Put(
        int id,
        EnderecoRequest request
    )
    {
        var clienteId = await ObterClienteIdAsync();

        if (clienteId is null)
        {
            return Unauthorized(new
            {
                mensagem = "Cliente não identificado."
            });
        }

        var endereco = await enderecoService.GetOwnedAsync(
            id,
            clienteId.Value
        );

        if (endereco is null)
        {
            return NotFound(new
            {
                mensagem = $"Endereço {id} não encontrado."
            });
        }

        var cidadeExiste = await db.Cidades
            .AsNoTracking()
            .AnyAsync(cidade =>
                cidade.Id == request.CidadeId
            );

        if (!cidadeExiste)
        {
            return BadRequest(new
            {
                mensagem =
                    $"A cidade {request.CidadeId} não existe."
            });
        }

        var dadosAtualizados = new Endereco
        {
            Rua = request.Rua.Trim(),
            Numero = request.Numero,
            Cep = request.Cep.Trim(),
            Complemento = request.Complemento?.Trim(),
            Telefone = request.Telefone,
            Bairro = request.Bairro.Trim(),
            CidadeId = request.CidadeId
        };

        await enderecoService.UpdateAsync(
            endereco,
            dadosAtualizados
        );

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> Delete(int id)
    {
        var clienteId = await ObterClienteIdAsync();

        if (clienteId is null)
        {
            return Unauthorized(new
            {
                mensagem = "Cliente não identificado."
            });
        }

        var endereco = await enderecoService.GetOwnedAsync(
            id,
            clienteId.Value
        );

        if (endereco is null)
        {
            return NotFound(new
            {
                mensagem = $"Endereço {id} não encontrado."
            });
        }

        await enderecoService.DeleteAsync(endereco);

        return NoContent();
    }
}

[ApiController]
[Route("cliente/me/pedido")]
[Authorize(Roles = "Cliente")]
public class PedidoCrudController(
    PedidoService pedidoService,
    ClienteService clienteService,
    AppDbContext db
) : ControllerBase
{
    private async Task<int?> ObterClienteIdAsync()
    {
        var claim = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (!int.TryParse(claim, out var usuarioId))
        {
            return null;
        }

        var cliente = await clienteService.GetByUserAsync(
            usuarioId
        );

        return cliente?.Id;
    }

    private async Task<PedidoDto> ParaDtoAsync(
        Pedido pedido
    )
    {
        var itens = await db.ItensPedido
            .AsNoTracking()
            .Where(item =>
                item.PedidoId == pedido.Id
            )
            .Join(
                db.Produtos.AsNoTracking(),
                item => item.ProdutoId,
                produto => produto.Id,
                (item, produto) => new ItemPedidoDto
                {
                    ProdutoId = produto.Id,
                    ProdutoNome = produto.Nome,
                    Quantidade = item.Quantidade,
                    ValorUnitario = item.ValorUnitario,
                    Subtotal =
                        item.Quantidade *
                        item.ValorUnitario
                }
            )
            .ToListAsync();

        return new PedidoDto
        {
            Id = pedido.Id,
            DataCadastro = pedido.DataCadastro,
            ValorTotal = pedido.ValorTotal,
            Status = pedido.StatusAtual.ToString(),
            Itens = itens
        };
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IEnumerable<PedidoDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    public async Task<IActionResult> Get()
    {
        var clienteId = await ObterClienteIdAsync();

        if (clienteId is null)
        {
            return Unauthorized(new
            {
                mensagem = "Cliente não identificado."
            });
        }

        var pedidos = await pedidoService.ForClienteAsync(
            clienteId.Value
        );

        var resultado = new List<PedidoDto>();

        foreach (var pedido in pedidos)
        {
            resultado.Add(
                await ParaDtoAsync(pedido)
            );
        }

        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(PedidoDto),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> GetById(int id)
    {
        var clienteId = await ObterClienteIdAsync();

        if (clienteId is null)
        {
            return Unauthorized(new
            {
                mensagem = "Cliente não identificado."
            });
        }

        var pedido = await pedidoService.OwnedAsync(
            id,
            clienteId.Value
        );

        if (pedido is null)
        {
            return NotFound(new
            {
                mensagem = $"Pedido {id} não encontrado."
            });
        }

        return Ok(
            await ParaDtoAsync(pedido)
        );
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(PedidoDto),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    public async Task<IActionResult> Post(
        PedidoRequest request
    )
    {
        var clienteId = await ObterClienteIdAsync();

        if (clienteId is null)
        {
            return Unauthorized(new
            {
                mensagem = "Cliente não identificado."
            });
        }

        var itensAgrupados = request.Itens
            .GroupBy(item => item.ProdutoId)
            .Select(grupo => new
            {
                ProdutoId = grupo.Key,
                Quantidade = grupo.Sum(
                    item => item.Quantidade
                )
            })
            .ToList();

        if (itensAgrupados.Count == 0)
        {
            return BadRequest(new
            {
                mensagem =
                    "O pedido deve possuir pelo menos um item."
            });
        }

        if (itensAgrupados.Any(item =>
                item.Quantidade < 1 ||
                item.Quantidade > 10000
            ))
        {
            return BadRequest(new
            {
                mensagem =
                    "A quantidade deve estar entre 1 e 10.000."
            });
        }

        var idsProdutos = itensAgrupados
            .Select(item => item.ProdutoId)
            .ToList();

        var produtos = await db.Produtos
            .AsNoTracking()
            .Where(produto =>
                idsProdutos.Contains(produto.Id)
            )
            .ToDictionaryAsync(
                produto => produto.Id
            );

        var idsInexistentes = idsProdutos
            .Where(id =>
                !produtos.ContainsKey(id)
            )
            .ToList();

        if (idsInexistentes.Count > 0)
        {
            return BadRequest(new
            {
                mensagem =
                    "Um ou mais produtos não existem.",

                produtosInexistentes =
                    idsInexistentes
            });
        }

        var itensPedido = itensAgrupados
            .Select(item =>
            {
                var produto = produtos[item.ProdutoId];

                return new ItemPedido
                {
                    ProdutoId = produto.Id,
                    Quantidade = item.Quantidade,

                    // O preço vem sempre do banco.
                    ValorUnitario =
                        produto.ValorUnitario
                };
            })
            .ToList();

        var pedido = new Pedido
        {
            ClienteId = clienteId.Value,
            DataCadastro = DateTime.UtcNow,

            StatusAtual =
                PedidoStatus.AGUARDANDO_PAGAMENTO
        };

        await pedidoService.CreateAsync(
            pedido,
            itensPedido
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = pedido.Id },
            await ParaDtoAsync(pedido)
        );
    }
} 