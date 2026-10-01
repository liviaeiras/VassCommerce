using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VassCommerce.Api.Data;
using VassCommerce.Api.Dtos;

namespace VassCommerce.Api.Controllers;

[ApiController]
[Route("cliente")]
[Authorize(Roles = "Cliente")]
public class ClienteController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClienteController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// GET /cliente/{id}
    /// Visualiza os dados do cliente (combinando Cliente + Usuario).
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<ClienteDto> GetCliente(int id)
    {
        var cliente = _db.Clientes.FirstOrDefault(c => c.Id == id);
        if (cliente is null || !ClientePertenceAoUsuario(id))
        {
            return NotFound(new
            {
                mensagem = $"Cliente {id} não encontrado."
            });
        }

        var usuario = _db.Usuarios.First(u => u.Id == cliente.UsuarioId);

        var dto = new ClienteDto
        {
            Id = cliente.Id,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            FotoUrl = usuario.FotoUrl,
            DataNascimento = cliente.DataNascimento,
            Cpf = cliente.Cpf
        };

        return Ok(dto);
    }

    /// <summary>
    /// GET /cliente/{idcliente}/formas-de-pagamento
    /// Lista os cartões (formas de pagamento) cadastrados pelo cliente.
    /// </summary>
    [HttpGet("{idcliente}/formas-de-pagamento")]
    public ActionResult<IEnumerable<CartaoDto>> GetFormasDePagamento(int idcliente)
    {
        if (!ClientePertenceAoUsuario(idcliente))
        {
            return NotFound(new { mensagem = $"Cliente {idcliente} não encontrado." });
        }

        var cartoes = _db.Cartoes
            .Where(c => c.ClienteId == idcliente && !c.Excluido)
            .Select(c => new CartaoDto
            {
                Id = c.Id,
                Tipo = c.Tipo,
                DataCriacao = c.DataCriacao
            })
            .ToList();

        return Ok(cartoes);
    }

    /// <summary>
    /// GET /cliente/{idcliente}/endereco
    /// Busca os dados do endereço do cliente.
    /// </summary>
    [HttpGet("{idcliente}/endereco")]
    public ActionResult<EnderecoDto> GetEndereco(int idcliente)
    {
        if (!ClientePertenceAoUsuario(idcliente))
        {
            return NotFound(new { mensagem = $"Cliente {idcliente} não encontrado." });
        }

        var endereco = _db.Enderecos.FirstOrDefault(e => e.ClienteId == idcliente);
        if (endereco is null)
        {
            return NotFound(new { mensagem = $"Cliente {idcliente} não possui endereço cadastrado." });
        }

        var cidade = _db.Cidades.First(c => c.Id == endereco.CidadeId);
        var estado = _db.Estados.First(e => e.Id == cidade.EstadoId);

        var dto = new EnderecoDto
        {
            Id = endereco.Id,
            Rua = endereco.Rua,
            Numero = endereco.Numero,
            Cep = endereco.Cep,
            Complemento = endereco.Complemento,
            Telefone = endereco.Telefone,
            Bairro = endereco.Bairro,
            Cidade = cidade.Nome,
            Estado = estado.Nome
        };

        return Ok(dto);
    }

    private bool ClientePertenceAoUsuario(int clienteId)
    {
        var claimId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        return int.TryParse(claimId, out var usuarioId) &&
            _db.Clientes.Any(
                cliente =>
                    cliente.Id == clienteId &&
                    cliente.UsuarioId == usuarioId
            );
    }
}
