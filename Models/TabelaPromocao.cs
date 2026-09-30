namespace VassCommerce.Api.Models;

public class TabelaPromocao
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime DataUltimaAtualizacao { get; set; }

    // Relacionamento "cadastrado por": todo administrador pode lançar uma tabela de promoção.
    public int AdministradorId { get; set; }
}
