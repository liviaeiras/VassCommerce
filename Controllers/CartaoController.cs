using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VassCommerce.Api.Dtos;
using VassCommerce.Api.Models;
using VassCommerce.Api.Services;

namespace VassCommerce.Api.Controllers;

[ApiController]
[Route("cliente/me/cartao")]
[Authorize(Roles = "Cliente")]
public class CartaoController(
    CartaoService cartaoService,
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

    private static CartaoDto ParaDto(Cartao cartao)
    {
        return new CartaoDto
        {
            Id = cartao.Id,
            Tipo = cartao.Tipo,
            DataCriacao = cartao.DataCriacao
        };
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(List<CartaoDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        var cartoes = await cartaoService.ForClienteAsync(
            clienteId.Value
        );

        return Ok(cartoes.Select(ParaDto).ToList());
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CartaoDto),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Post(CartaoRequest request)
    {
        var clienteId = await ObterClienteIdAsync();

        if (clienteId is null)
        {
            return Unauthorized(new
            {
                mensagem = "Cliente não identificado."
            });
        }

        if (request.Tipo != TipoCartao.CREDITO &&
            request.Tipo != TipoCartao.DEBITO)
        {
            return BadRequest(new
            {
                mensagem =
                    "Informe um tipo de cartão válido: crédito ou débito."
            });
        }

        var cartao = new Cartao
        {
            ClienteId = clienteId.Value,
            Tipo = request.Tipo,
            DataCriacao = DateTime.UtcNow,
            Excluido = false
        };

        await cartaoService.SaveAsync(cartao);

        return CreatedAtAction(
            nameof(Get),
            null,
            ParaDto(cartao)
        );
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        var cartao = await cartaoService.GetOwnedAsync(
            id,
            clienteId.Value
        );

        if (cartao is null)
        {
            return NotFound(new
            {
                mensagem = $"Cartão {id} não encontrado."
            });
        }

        await cartaoService.DeleteAsync(cartao);

        return NoContent();
    }
}