using Microsoft.AspNetCore.Mvc;
using VassCommerce.Api.Data;
using VassCommerce.Api.Dtos;

namespace VassCommerce.Api.Controllers;

[ApiController]
[Route("estado")]
public class EstadoController : ControllerBase
{
    private readonly AppDbContext _db;

    public EstadoController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// GET /estado
    /// Lista todos os estados cadastrados.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<EstadoDto>> GetEstados()
    {
        var estados = _db.Estados
            .Select(e => new EstadoDto { Id = e.Id, Sigla = e.Sigla, Nome = e.Nome })
            .ToList();

        return Ok(estados);
    }

    /// <summary>
    /// GET /estado/{idestado}/cidade
    /// Lista todas as cidades de um estado.
    /// </summary>
    [HttpGet("{idestado}/cidade")]
    public ActionResult<IEnumerable<CidadeDto>> GetCidadesDoEstado(int idestado)
    {
        var estado = _db.Estados.FirstOrDefault(e => e.Id == idestado);
        if (estado is null)
        {
            return NotFound(new { mensagem = $"Estado {idestado} não encontrado." });
        }

        var cidades = _db.Cidades
            .Where(c => c.EstadoId == idestado)
            .Select(c => new CidadeDto { Id = c.Id, Nome = c.Nome })
            .ToList();

        return Ok(cidades);
    }
}

