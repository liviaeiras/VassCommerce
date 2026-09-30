using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VassCommerce.Api.Data;
using VassCommerce.Api.Dtos;
using VassCommerce.Api.Models;
using VassCommerce.Api.Services;

namespace VassCommerce.Api.Controllers;

[ApiController]
[Route("produto")]
public class ProdutoController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProdutoService _service;

    public ProdutoController(
        AppDbContext db,
        ProdutoService service
    )
    {
        _db = db;
        _service = service;
    }

    /// <summary>
    /// Retorna os dados de um produto pelo ID.
    /// A consulta é pública.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ProdutoDetalheDto),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public ActionResult<ProdutoDetalheDto> GetProduto(int id)
    {
        var produto = _db.Produtos.FirstOrDefault(
            produto => produto.Id == id
        );

        if (produto is null)
        {
            return NotFound(new
            {
                mensagem = $"Produto {id} não encontrado."
            });
        }

        return Ok(ToDto(produto));
    }

    /// <summary>
    /// Cadastra um produto.
    /// Somente administradores podem utilizar este endpoint.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ProducesResponseType(
        typeof(ProdutoDetalheDto),
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
    public async Task<ActionResult<ProdutoDetalheDto>> Create(
        ProdutoRequest request
    )
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return BadRequest(new
            {
                mensagem = "O nome do produto é obrigatório."
            });
        }

        if (request.ValorUnitario <= 0)
        {
            return BadRequest(new
            {
                mensagem =
                    "O valor do produto deve ser maior que zero."
            });
        }

        var categoriaExiste = _db.Categorias.Any(
            categoria =>
                categoria.Id == request.CategoriaId
        );

        if (!categoriaExiste)
        {
            return BadRequest(new
            {
                mensagem =
                    $"A categoria {request.CategoriaId} não existe."
            });
        }

        var agora = DateTime.UtcNow;

        var produto = new Produto
        {
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao.Trim(),
            FotoUrl = request.FotoUrl.Trim(),
            ValorUnitario = request.ValorUnitario,
            CategoriaId = request.CategoriaId,
            DataCadastro = agora,
            DataUltimaAtualizacao = agora
        };

        await _service.SaveAsync(produto);

        var dto = ToDto(produto);

        return CreatedAtAction(
            nameof(GetProduto),
            new { id = produto.Id },
            dto
        );
    }

    /// <summary>
    /// Atualiza um produto.
    /// Somente administradores podem utilizar este endpoint.
    /// </summary>
    [Authorize(Roles = "Administrador")]
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
    public async Task<IActionResult> Update(
        int id,
        ProdutoRequest request
    )
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return BadRequest(new
            {
                mensagem = "O nome do produto é obrigatório."
            });
        }

        if (request.ValorUnitario <= 0)
        {
            return BadRequest(new
            {
                mensagem =
                    "O valor do produto deve ser maior que zero."
            });
        }

        var categoriaExiste = _db.Categorias.Any(
            categoria =>
                categoria.Id == request.CategoriaId
        );

        if (!categoriaExiste)
        {
            return BadRequest(new
            {
                mensagem =
                    $"A categoria {request.CategoriaId} não existe."
            });
        }

        var produtoAtualizado = new Produto
        {
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao.Trim(),
            FotoUrl = request.FotoUrl.Trim(),
            ValorUnitario = request.ValorUnitario,
            CategoriaId = request.CategoriaId,
            DataUltimaAtualizacao = DateTime.UtcNow
        };

        var atualizado = await _service.UpdateAsync(
            id,
            produtoAtualizado
        );

        if (!atualizado)
        {
            return NotFound(new
            {
                mensagem = $"Produto {id} não encontrado."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Exclui um produto.
    /// Somente administradores podem utilizar este endpoint.
    /// </summary>
    [Authorize(Roles = "Administrador")]
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
        var excluido = await _service.DeleteAsync(id);

        if (!excluido)
        {
            return NotFound(new
            {
                mensagem = $"Produto {id} não encontrado."
            });
        }

        return NoContent();
    }

    private ProdutoDetalheDto ToDto(Produto produto)
    {
        var categoriaNome = _db.Categorias
            .Where(categoria =>
                categoria.Id == produto.CategoriaId
            )
            .Select(categoria => categoria.Nome)
            .FirstOrDefault();

        return new ProdutoDetalheDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Descricao = produto.Descricao,
            FotoUrl = produto.FotoUrl,
            ValorUnitario = produto.ValorUnitario,

            ValorComPromocao =
                PrecoService.ObterMenorPrecoPromocional(
                    _db,
                    produto.Id
                ),

            DataCadastro = produto.DataCadastro,
            DataUltimaAtualizacao =
                produto.DataUltimaAtualizacao,

            CategoriaNome =
                categoriaNome ?? string.Empty
        };
    }
}