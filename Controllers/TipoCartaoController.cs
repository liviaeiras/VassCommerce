using Microsoft.AspNetCore.Mvc;
using VassCommerce.Api.Dtos;
using VassCommerce.Api.Models;

namespace VassCommerce.Api.Controllers;

[ApiController]
[Route("tipo-cartao")]
public class TipoCartaoController : ControllerBase
{
    /// <summary>
    /// GET /tipo-cartao
    /// Lista todos os tipos de cartão suportados (enum do diagrama).
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<TipoCartaoDto>> GetTiposCartao()
    {
        var tipos = Enum.GetValues<TipoCartao>()
            .Select(t => new TipoCartaoDto
            {
                Id = (int)t,
                Nome = t.ToString()
            })
            .ToList();

        return Ok(tipos);
    }
}

