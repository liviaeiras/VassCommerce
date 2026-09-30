using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VassCommerce.Api.Data;
using VassCommerce.Api.Dtos;
using VassCommerce.Api.Models;
using VassCommerce.Api.Services;

namespace VassCommerce.Api.Controllers;

[ApiController]
[Route("categoria")]
public class CategoriaController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CategoriaService _service;

    public CategoriaController(
        AppDbContext db,
        CategoriaService service
    )
    {
        _db = db;
        _service = service;
    }

    /// <summary>
    /// Lista todas as categorias.
    /// É possível pesquisar pelo nome.
    /// Este endpoint é público.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(
        typeof(IEnumerable<CategoriaDto>),
        StatusCodes.Status200OK
    )]
    public ActionResult<IEnumerable<CategoriaDto>> GetCategorias(
        [FromQuery] string? nome
    )
    {
        var categorias = _db.Categorias
            .AsNoTracking()
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var comparador =
                CultureInfo.InvariantCulture.CompareInfo;

            const CompareOptions opcoes =
                CompareOptions.IgnoreCase |
                CompareOptions.IgnoreNonSpace;

            categorias = categorias.Where(
                categoria =>
                    comparador.IndexOf(
                        categoria.Nome,
                        nome.Trim(),
                        opcoes
                    ) >= 0
            );
        }

        var resultado = categorias
            .OrderBy(categoria => categoria.Nome)
            .Select(ToDto)
            .ToList();

        return Ok(resultado);
    }

    /// <summary>
    /// Retorna uma categoria específica pelo ID.
    /// Este endpoint é público.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(CategoriaDto),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<CategoriaDto>> GetCategoria(
        int id
    )
    {
        var categoria = await _db.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(
                categoria => categoria.Id == id
            );

        if (categoria is null)
        {
            return NotFound(new
            {
                mensagem = $"Categoria {id} não encontrada."
            });
        }

        return Ok(ToDto(categoria));
    }

    /// <summary>
    /// Lista os produtos pertencentes a uma categoria.
    /// Este endpoint é público.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{idCategoria:int}/produto")]
    [ProducesResponseType(
        typeof(IEnumerable<ProdutoResumoDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<IEnumerable<ProdutoResumoDto>>>
        GetProdutosDaCategoria(int idCategoria)
    {
        var categoriaExiste = await _db.Categorias
            .AsNoTracking()
            .AnyAsync(
                categoria => categoria.Id == idCategoria
            );

        if (!categoriaExiste)
        {
            return NotFound(new
            {
                mensagem =
                    $"Categoria {idCategoria} não encontrada."
            });
        }

        var produtos = await _db.Produtos
            .AsNoTracking()
            .Where(
                produto =>
                    produto.CategoriaId == idCategoria
            )
            .OrderBy(produto => produto.Nome)
            .ToListAsync();

        var resultado = produtos
            .Select(produto => new ProdutoResumoDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                ValorUnitario = produto.ValorUnitario,

                ValorComPromocao =
                    PrecoService.ObterMenorPrecoPromocional(
                        _db,
                        produto.Id
                    )
            })
            .ToList();

        return Ok(resultado);
    }

    /// <summary>
    /// Cadastra uma categoria.
    /// Somente administradores podem utilizar este endpoint.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ProducesResponseType(
        typeof(CategoriaDto),
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
    [ProducesResponseType(
        StatusCodes.Status409Conflict
    )]
    public async Task<ActionResult<CategoriaDto>> Create(
        CategoriaRequest request
    )
    {
        var nome = request.Nome.Trim();

        var categoriaJaExiste = await _db.Categorias.AnyAsync(
            categoria => categoria.Nome == nome
        );

        if (categoriaJaExiste)
        {
            return Conflict(new
            {
                mensagem =
                    $"A categoria '{nome}' já está cadastrada."
            });
        }

        var categoria = new Categoria
        {
            Nome = nome,
            Descricao = request.Descricao.Trim(),
            ImagemSimboloUrl =
                request.ImagemSimboloUrl.Trim()
        };

        await _service.SaveAsync(categoria);

        var dto = ToDto(categoria);

        return CreatedAtAction(
            nameof(GetCategoria),
            new { id = categoria.Id },
            dto
        );
    }

    /// <summary>
    /// Atualiza uma categoria.
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
    [ProducesResponseType(
        StatusCodes.Status409Conflict
    )]
    public async Task<IActionResult> Update(
        int id,
        CategoriaRequest request
    )
    {
        var categoriaExiste = await _db.Categorias.AnyAsync(
            categoria => categoria.Id == id
        );

        if (!categoriaExiste)
        {
            return NotFound(new
            {
                mensagem = $"Categoria {id} não encontrada."
            });
        }

        var nome = request.Nome.Trim();

        var nomeJaUtilizado = await _db.Categorias.AnyAsync(
            categoria =>
                categoria.Id != id &&
                categoria.Nome == nome
        );

        if (nomeJaUtilizado)
        {
            return Conflict(new
            {
                mensagem =
                    $"Já existe outra categoria chamada '{nome}'."
            });
        }

        var categoriaAtualizada = new Categoria
        {
            Nome = nome,
            Descricao = request.Descricao.Trim(),
            ImagemSimboloUrl =
                request.ImagemSimboloUrl.Trim()
        };

        var atualizado = await _service.UpdateAsync(
            id,
            categoriaAtualizada
        );

        if (!atualizado)
        {
            return NotFound(new
            {
                mensagem = $"Categoria {id} não encontrada."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Exclui uma categoria sem vínculos.
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
    [ProducesResponseType(
        StatusCodes.Status409Conflict
    )]
    public async Task<IActionResult> Delete(int id)
    {
        var categoriaExiste = await _db.Categorias.AnyAsync(
            categoria => categoria.Id == id
        );

        if (!categoriaExiste)
        {
            return NotFound(new
            {
                mensagem = $"Categoria {id} não encontrada."
            });
        }

        var possuiProdutos = await _db.Produtos.AnyAsync(
            produto => produto.CategoriaId == id
        );

        if (possuiProdutos)
        {
            return Conflict(new
            {
                mensagem =
                    "A categoria não pode ser excluída " +
                    "porque possui produtos vinculados."
            });
        }

        var possuiAdministrador =
            await _db.Administradores.AnyAsync(
                administrador =>
                    administrador.CategoriaId == id
            );

        if (possuiAdministrador)
        {
            return Conflict(new
            {
                mensagem =
                    "A categoria não pode ser excluída " +
                    "porque possui um administrador responsável."
            });
        }

        var excluido = await _service.DeleteAsync(id);

        if (!excluido)
        {
            return NotFound(new
            {
                mensagem = $"Categoria {id} não encontrada."
            });
        }

        return NoContent();
    }

    private static CategoriaDto ToDto(
        Categoria categoria
    )
    {
        return new CategoriaDto
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao,
            ImagemSimboloUrl =
                categoria.ImagemSimboloUrl
        };
    }
}