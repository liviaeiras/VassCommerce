using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VassCommerce.Api.Dtos;
using VassCommerce.Api.Models;
using VassCommerce.Api.Services;

namespace VassCommerce.Api.Controllers;

[ApiController]
[Route("cliente/me/pedido/{pedidoId:int}/pagamento")]
[Authorize(Roles = "Cliente")]
public class PagamentoController(
    PagamentoService pagamentoService,
    ClienteService clienteService
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

    private static PagamentoDto ParaDto(
        Pagamento pagamento
    )
    {
        return new PagamentoDto
        {
            Id = pagamento.Id,
            PedidoId = pagamento.PedidoId,
            ValorPago = pagamento.ValorPago
        };
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagamentoDto),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(int pedidoId)
    {
        var clienteId = await ObterClienteIdAsync();

        if (clienteId is null)
        {
            return Unauthorized(new
            {
                mensagem = "Cliente não identificado."
            });
        }

        Pagamento? pagamento;

        try
        {
            pagamento = await pagamentoService.GetOwnedAsync(
                pedidoId,
                clienteId.Value
            );
        }
        catch
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensagem =
                        "Não foi possível consultar o pagamento."
                }
            );
        }

        if (pagamento is null)
        {
            return NotFound(new
            {
                mensagem =
                    $"Pagamento do pedido {pedidoId} não encontrado."
            });
        }

        return Ok(ParaDto(pagamento));
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(PagamentoDto),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Post(int pedidoId)
    {
        var clienteId = await ObterClienteIdAsync();

        if (clienteId is null)
        {
            return Unauthorized(new
            {
                mensagem = "Cliente não identificado."
            });
        }

        ResultadoCriacaoPagamento resultado;

        try
        {
            resultado = await pagamentoService.CreateAsync(
                pedidoId,
                clienteId.Value
            );
        }
        catch
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensagem =
                        "Não foi possível registrar o pagamento."
                }
            );
        }

        if (resultado.Resultado ==
            ResultadoPagamento.PedidoNaoEncontrado)
        {
            return NotFound(new
            {
                mensagem = $"Pedido {pedidoId} não encontrado."
            });
        }

        if (resultado.Resultado ==
            ResultadoPagamento.PagamentoJaRegistrado)
        {
            return Conflict(new
            {
                mensagem = "O pedido já possui um pagamento registrado."
            });
        }

        if (resultado.Resultado ==
            ResultadoPagamento.PedidoJaPago)
        {
            return Conflict(new
            {
                mensagem = "O pedido já foi pago."
            });
        }

        if (resultado.Resultado ==
            ResultadoPagamento.EstadoInvalido)
        {
            return BadRequest(new
            {
                mensagem =
                    "O pedido não está aguardando pagamento."
            });
        }

        if (resultado.Resultado ==
            ResultadoPagamento.CartaoNaoEncontrado)
        {
            return BadRequest(new
            {
                mensagem =
                    "O cliente não possui um cartão válido cadastrado."
            });
        }

        var pagamento = resultado.Pagamento!;

        return CreatedAtAction(
            nameof(Get),
            new { pedidoId },
            ParaDto(pagamento)
        );
    }
}