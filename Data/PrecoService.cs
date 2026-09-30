using VassCommerce.Api.Models;

namespace VassCommerce.Api.Data;

public static class PrecoService
{
    public static decimal? ObterMenorPrecoPromocional(AppDbContext db, int produtoId)
    {
        var hoje = DateTime.Now;

        var precosAtivos = from item in db.PromocaoItens
                            join tabela in db.TabelasPromocao on item.TabelaPromocaoId equals tabela.Id
                            where item.ProdutoId == produtoId
                                  && tabela.DataInicio <= hoje
                                  && tabela.DataFim >= hoje
                            select item.ValorPromocao;

        return precosAtivos.Any() ? precosAtivos.Min() : (decimal?)null;
    }
}
