using Microsoft.EntityFrameworkCore;
using VassCommerce.Api.Data;
using VassCommerce.Api.Models;

namespace VassCommerce.Api.Data
{

public class DataStore
{
    public List<Estado> Estados { get; } = new();
    public List<Cidade> Cidades { get; } = new();
    public List<Endereco> Enderecos { get; } = new();
    public List<Categoria> Categorias { get; } = new();
    public List<Produto> Produtos { get; } = new();
    public List<TabelaPromocao> TabelasPromocao { get; } = new();
    public List<PromocaoItem> PromocaoItens { get; } = new();
    public List<Usuario> Usuarios { get; } = new();
    public List<Cliente> Clientes { get; } = new();
    public List<Administrador> Administradores { get; } = new();
    public List<Cartao> Cartoes { get; } = new();
    public List<Pedido> Pedidos { get; } = new();
    public List<ItemPedido> ItensPedido { get; } = new();
    public List<Pagamento> Pagamentos { get; } = new();

    public DataStore()
    {
        Seed();
    }

    private void Seed()
    {
        var agora = DateTime.Now;

        // ---------- Estado / Cidade ----------
        Estados.AddRange(new[]
        {
            new Estado { Id = 1, Sigla = "RJ", Nome = "Rio de Janeiro" },
            new Estado { Id = 2, Sigla = "SP", Nome = "São Paulo" }
        });

        Cidades.AddRange(new[]
        {
            new Cidade { Id = 1, Nome = "Rio de Janeiro", EstadoId = 1 },
            new Cidade { Id = 2, Nome = "Vassouras", EstadoId = 1 },
            new Cidade { Id = 3, Nome = "São Paulo", EstadoId = 2 }
        });

        // ---------- Categoria ----------
        Categorias.AddRange(new[]
        {
            new Categoria { Id = 1, Nome = "Informática", Descricao = "Computadores, periféricos e acessórios", ImagemSimboloUrl = "/img/categorias/informatica.png" },
            new Categoria { Id = 2, Nome = "Livros", Descricao = "Livros em geral", ImagemSimboloUrl = "/img/categorias/livros.png" }
        });

        // ---------- Usuario / Administrador ----------
        Usuarios.AddRange(new[]
        {
            new Usuario { Id = 1, NomeCompleto = "Ana Souza", Email = "ana.souza@vasscommerce.com", Senha = "hash-fake-1", FotoUrl = "/img/usuarios/1.png", DataCadastro = agora.AddYears(-2), DataUltimaAtualizacao = agora.AddMonths(-1) },
            new Usuario { Id = 2, NomeCompleto = "Bruno Lima", Email = "bruno.lima@vasscommerce.com", Senha = "hash-fake-2", FotoUrl = "/img/usuarios/2.png", DataCadastro = agora.AddYears(-1), DataUltimaAtualizacao = agora.AddMonths(-2) },
            new Usuario { Id = 3, NomeCompleto = "Carla Dias", Email = "carla.dias@gmail.com", Senha = "hash-fake-3", FotoUrl = "/img/usuarios/3.png", DataCadastro = agora.AddMonths(-8), DataUltimaAtualizacao = agora.AddDays(-10) },
            new Usuario { Id = 4, NomeCompleto = "Diego Alves", Email = "diego.alves@gmail.com", Senha = "hash-fake-4", FotoUrl = "/img/usuarios/4.png", DataCadastro = agora.AddMonths(-5), DataUltimaAtualizacao = agora.AddDays(-3) }
        });

        Administradores.AddRange(new[]
        {
            new Administrador { Id = 1, UsuarioId = 1, CategoriaId = 1 }, // Ana -> Informática
            new Administrador { Id = 2, UsuarioId = 2, CategoriaId = 2 }  // Bruno -> Livros
        });

        // ---------- Cliente / Endereco ----------
        Clientes.AddRange(new[]
        {
            new Cliente { Id = 1, UsuarioId = 3, DataNascimento = new DateTime(1995, 4, 12), Cpf = "111.111.111-11" },
            new Cliente { Id = 2, UsuarioId = 4, DataNascimento = new DateTime(1990, 9, 30), Cpf = "222.222.222-22" }
        });

        Enderecos.AddRange(new[]
        {
            new Endereco { Id = 1, Rua = "Rua das Flores", Numero = 100, Cep = "20000-000", Complemento = "Apto 301", Telefone = 21999990000, Bairro = "Centro", CidadeId = 1, ClienteId = 1, DataCadastro = agora.AddMonths(-8), DataUltimaAtualizacao = agora.AddMonths(-8) },
            new Endereco { Id = 2, Rua = "Av. Barão do Amazonas", Numero = 200, Cep = "27700-000", Complemento = "", Telefone = 24988880000, Bairro = "Centro", CidadeId = 2, ClienteId = 2, DataCadastro = agora.AddMonths(-5), DataUltimaAtualizacao = agora.AddMonths(-5) }
        });

        // ---------- Produto ----------
        Produtos.AddRange(new[]
        {
            new Produto { Id = 1, Nome = "Notebook 15\"", Descricao = "Notebook 15 polegadas, 16GB RAM, SSD 512GB", FotoUrl = "/img/produtos/1.png", ValorUnitario = 4500.00m, CategoriaId = 1, DataCadastro = agora.AddMonths(-6), DataUltimaAtualizacao = agora.AddDays(-20) },
            new Produto { Id = 2, Nome = "Mouse sem fio", Descricao = "Mouse óptico sem fio", FotoUrl = "/img/produtos/2.png", ValorUnitario = 89.90m, CategoriaId = 1, DataCadastro = agora.AddMonths(-6), DataUltimaAtualizacao = agora.AddDays(-15) },
            new Produto { Id = 3, Nome = "Clean Code", Descricao = "Livro sobre boas práticas de programação", FotoUrl = "/img/produtos/3.png", ValorUnitario = 120.00m, CategoriaId = 2, DataCadastro = agora.AddMonths(-4), DataUltimaAtualizacao = agora.AddDays(-30) }
        });

        // ---------- TabelaPromocao / PromocaoItem ----------
        TabelasPromocao.AddRange(new[]
        {
            new TabelaPromocao
            {
                Id = 1,
                Nome = "Black Friday",
                Descricao = "Promoção de Black Friday em Informática",
                DataInicio = agora.AddDays(-5),
                DataFim = agora.AddDays(5),
                DataCadastro = agora.AddDays(-10),
                DataUltimaAtualizacao = agora.AddDays(-10),
                AdministradorId = 1
            },
            new TabelaPromocao
            {
                Id = 2,
                Nome = "Semana do Livro",
                Descricao = "Descontos em livros selecionados",
                DataInicio = agora.AddDays(-2),
                DataFim = agora.AddDays(10),
                DataCadastro = agora.AddDays(-3),
                DataUltimaAtualizacao = agora.AddDays(-3),
                AdministradorId = 2
            }
        });

        PromocaoItens.AddRange(new[]
        {
            new PromocaoItem { Id = 1, ProdutoId = 1, TabelaPromocaoId = 1, ValorPromocao = 3999.00m, DataCadastro = agora.AddDays(-10), DataUltimaAtualizacao = agora.AddDays(-10) },
            new PromocaoItem { Id = 2, ProdutoId = 3, TabelaPromocaoId = 2, ValorPromocao = 99.90m, DataCadastro = agora.AddDays(-3), DataUltimaAtualizacao = agora.AddDays(-3) }
        });

        // ---------- Cartao ----------
        Cartoes.AddRange(new[]
        {
            new Cartao { Id = 1, ClienteId = 1, Tipo = TipoCartao.CREDITO, DataCriacao = agora.AddMonths(-7), Excluido = false },
            new Cartao { Id = 2, ClienteId = 1, Tipo = TipoCartao.DEBITO, DataCriacao = agora.AddMonths(-7), Excluido = false },
            new Cartao { Id = 3, ClienteId = 2, Tipo = TipoCartao.CREDITO, DataCriacao = agora.AddMonths(-4), Excluido = false }
        });

        // ---------- Pedido / ItemPedido / Pagamento ----------
        Pedidos.AddRange(new[]
        {
            new Pedido { Id = 1, ClienteId = 1, DataCadastro = agora.AddDays(-3), ValorTotal = 4589.90m, StatusAtual = PedidoStatus.SEPARANDO_ESTOQUE },
            new Pedido { Id = 2, ClienteId = 2, DataCadastro = agora.AddDays(-1), ValorTotal = 120.00m, StatusAtual = PedidoStatus.AGUARDANDO_PAGAMENTO }
        });

        ItensPedido.AddRange(new[]
        {
            new ItemPedido { Id = 1, PedidoId = 1, ProdutoId = 1, Quantidade = 1, ValorUnitario = 4500.00m },
            new ItemPedido { Id = 2, PedidoId = 1, ProdutoId = 2, Quantidade = 1, ValorUnitario = 89.90m },
            new ItemPedido { Id = 3, PedidoId = 2, ProdutoId = 3, Quantidade = 1, ValorUnitario = 120.00m }
        });

        Pagamentos.AddRange(new[]
        {
            // Pedido 1 pago em duas partes, com dois cartões diferentes do mesmo cliente.
            new Pagamento { Id = 1, PedidoId = 1, ClienteId = 1, CartaoId = 1, ValorPago = 4000.00m },
            new Pagamento { Id = 2, PedidoId = 1, ClienteId = 1, CartaoId = 2, ValorPago = 589.90m }
        });
    }
}

}

namespace VassCommerce.Api.Services
{

public class CategoriaService(AppDbContext db)
{
    public Task<List<Categoria>> ListAsync()
    {
        return db.Categorias
            .AsNoTracking()
            .OrderBy(categoria => categoria.Nome)
            .ToListAsync();
    }

    public Task<Categoria?> GetAsync(int id)
    {
        return db.Categorias
            .FindAsync(id)
            .AsTask();
    }

    public async Task<Categoria> SaveAsync(Categoria categoria)
    {
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync();

        return categoria;
    }

    public async Task<bool> UpdateAsync(
        int id,
        Categoria input
    )
    {
        var categoria = await db.Categorias.FindAsync(id);

        if (categoria is null)
        {
            return false;
        }

        categoria.Nome = input.Nome;
        categoria.Descricao = input.Descricao;
        categoria.ImagemSimboloUrl = input.ImagemSimboloUrl;

        await db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var categoria = await db.Categorias.FindAsync(id);

        if (categoria is null)
        {
            return false;
        }

        db.Categorias.Remove(categoria);
        await db.SaveChangesAsync();

        return true;
    }
}

public class ProdutoService(AppDbContext db)
{
    public Task<List<Produto>> ListAsync()
    {
        return db.Produtos
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Produto?> GetAsync(int id)
    {
        return db.Produtos
            .FindAsync(id)
            .AsTask();
    }

    public async Task<Produto> SaveAsync(Produto produto)
    {
        db.Produtos.Add(produto);
        await db.SaveChangesAsync();

        return produto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        Produto input
    )
    {
        var produto = await db.Produtos.FindAsync(id);

        if (produto is null)
        {
            return false;
        }

        produto.Nome = input.Nome;
        produto.Descricao = input.Descricao;
        produto.FotoUrl = input.FotoUrl;
        produto.ValorUnitario = input.ValorUnitario;
        produto.CategoriaId = input.CategoriaId;
        produto.DataUltimaAtualizacao = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var produto = await db.Produtos.FindAsync(id);

        if (produto is null)
        {
            return false;
        }

        db.Produtos.Remove(produto);
        await db.SaveChangesAsync();

        return true;
    }
}

public class EnderecoService(AppDbContext db)
{
    public Task<List<Endereco>> ForClienteAsync(int clienteId)
    {
        return db.Enderecos
            .AsNoTracking()
            .Where(endereco =>
                endereco.ClienteId == clienteId
            )
            .ToListAsync();
    }

    public Task<Endereco?> GetOwnedAsync(
        int id,
        int clienteId
    )
    {
        return db.Enderecos.FirstOrDefaultAsync(
            endereco =>
                endereco.Id == id &&
                endereco.ClienteId == clienteId
        );
    }

    public async Task<Endereco> SaveAsync(Endereco endereco)
    {
        db.Enderecos.Add(endereco);
        await db.SaveChangesAsync();

        return endereco;
    }

    public async Task<bool> UpdateAsync(
        Endereco endereco,
        Endereco input
    )
    {
        endereco.Rua = input.Rua;
        endereco.Numero = input.Numero;
        endereco.Cep = input.Cep;
        endereco.Complemento = input.Complemento;
        endereco.Telefone = input.Telefone;
        endereco.Bairro = input.Bairro;
        endereco.CidadeId = input.CidadeId;
        endereco.DataUltimaAtualizacao = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return true;
    }

    public async Task DeleteAsync(Endereco endereco)
    {
        db.Enderecos.Remove(endereco);
        await db.SaveChangesAsync();
    }
}

public class ClienteService(AppDbContext db)
{
    public Task<Cliente?> GetByUserAsync(int userId)
    {
        return db.Clientes.FirstOrDefaultAsync(
            cliente => cliente.UsuarioId == userId
        );
    }

    public Task<Usuario?> UserAsync(int id)
    {
        return db.Usuarios
            .FindAsync(id)
            .AsTask();
    }
}

public class PedidoService(AppDbContext db)
{
    public Task<List<Pedido>> ForClienteAsync(int clienteId)
    {
        return db.Pedidos
            .AsNoTracking()
            .Where(pedido =>
                pedido.ClienteId == clienteId
            )
            .ToListAsync();
    }

    public Task<Pedido?> OwnedAsync(
        int pedidoId,
        int clienteId
    )
    {
        return db.Pedidos.FirstOrDefaultAsync(
            pedido =>
                pedido.Id == pedidoId &&
                pedido.ClienteId == clienteId
        );
    }

    public async Task<Pedido> CreateAsync(
        Pedido pedido,
        IEnumerable<ItemPedido> itens
    )
    {
        var listaItens = itens.ToList();

        pedido.ValorTotal = listaItens.Sum(
            item =>
                item.Quantidade * item.ValorUnitario
        );

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        try
        {
            db.Pedidos.Add(pedido);
            await db.SaveChangesAsync();

            foreach (var item in listaItens)
            {
                item.PedidoId = pedido.Id;
            }

            db.ItensPedido.AddRange(listaItens);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return pedido;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

public enum ResultadoPagamento
{
    Criado,
    PedidoNaoEncontrado,
    PagamentoJaRegistrado,
    PedidoJaPago,
    EstadoInvalido,
    CartaoNaoEncontrado
}

public sealed class ResultadoCriacaoPagamento
{
    public ResultadoPagamento Resultado { get; init; }
    public Pagamento? Pagamento { get; init; }
}

public class PagamentoService(AppDbContext db)
{
    public Task<Pagamento?> GetOwnedAsync(
        int pedidoId,
        int clienteId
    )
    {
        return db.Pagamentos
            .AsNoTracking()
            .Join(
                db.Pedidos.AsNoTracking(),
                pagamento => pagamento.PedidoId,
                pedido => pedido.Id,
                (pagamento, pedido) => new
                {
                    Pagamento = pagamento,
                    pedido.ClienteId
                }
            )
            .Where(resultado =>
                resultado.Pagamento.PedidoId == pedidoId &&
                resultado.ClienteId == clienteId
            )
            .Select(resultado => resultado.Pagamento)
            .FirstOrDefaultAsync();
    }

    public async Task<ResultadoCriacaoPagamento> CreateAsync(
        int pedidoId,
        int clienteId
    )
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync();

        try
        {
            var pedido = await db.Pedidos.FirstOrDefaultAsync(
                item =>
                    item.Id == pedidoId &&
                    item.ClienteId == clienteId
            );

            if (pedido is null)
            {
                return new ResultadoCriacaoPagamento
                {
                    Resultado = ResultadoPagamento.PedidoNaoEncontrado
                };
            }

            var pagamentoExiste = await db.Pagamentos
                .AnyAsync(item => item.PedidoId == pedidoId);

            if (pagamentoExiste)
            {
                return new ResultadoCriacaoPagamento
                {
                    Resultado =
                        ResultadoPagamento.PagamentoJaRegistrado
                };
            }

            if (pedido.StatusAtual == PedidoStatus.PAGO)
            {
                return new ResultadoCriacaoPagamento
                {
                    Resultado = ResultadoPagamento.PedidoJaPago
                };
            }

            if (pedido.StatusAtual !=
                PedidoStatus.AGUARDANDO_PAGAMENTO)
            {
                return new ResultadoCriacaoPagamento
                {
                    Resultado = ResultadoPagamento.EstadoInvalido
                };
            }

            var cartao = await db.Cartoes
                .AsNoTracking()
                .Where(item =>
                    item.ClienteId == clienteId &&
                    !item.Excluido
                )
                .OrderBy(item => item.Id)
                .FirstOrDefaultAsync();

            if (cartao is null)
            {
                return new ResultadoCriacaoPagamento
                {
                    Resultado =
                        ResultadoPagamento.CartaoNaoEncontrado
                };
            }

            var pagamento = new Pagamento
            {
                PedidoId = pedido.Id,
                ClienteId = clienteId,
                CartaoId = cartao.Id,
                ValorPago = pedido.ValorTotal
            };

            pedido.StatusAtual = PedidoStatus.PAGO;
            db.Pagamentos.Add(pagamento);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultadoCriacaoPagamento
            {
                Resultado = ResultadoPagamento.Criado,
                Pagamento = pagamento
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

public class CartaoService(AppDbContext db)
{
    public Task<List<Cartao>> ForClienteAsync(int clienteId)
    {
        return db.Cartoes
            .AsNoTracking()
            .Where(cartao =>
                cartao.ClienteId == clienteId &&
                !cartao.Excluido
            )
            .OrderByDescending(cartao => cartao.DataCriacao)
            .ToListAsync();
    }

    public Task<Cartao?> GetOwnedAsync(
        int cartaoId,
        int clienteId
    )
    {
        return db.Cartoes.FirstOrDefaultAsync(
            cartao =>
                cartao.Id == cartaoId &&
                cartao.ClienteId == clienteId &&
                !cartao.Excluido
        );
    }

    public async Task<Cartao> SaveAsync(Cartao cartao)
    {
        db.Cartoes.Add(cartao);
        await db.SaveChangesAsync();

        return cartao;
    }

    public async Task DeleteAsync(Cartao cartao)
    {
        cartao.Excluido = true;
        await db.SaveChangesAsync();
    }
}

public class AuthService(AppDbContext db)
{
    public Usuario? Validate(
        string email,
        string senha
    )
    {
        var emailNormalizado = email
            .Trim()
            .ToLowerInvariant();

        var usuario = db.Usuarios.FirstOrDefault(
            usuario => usuario.Email == emailNormalizado
        );

        if (usuario is null)
        {
            return null;
        }

        var senhaValida = BCrypt.Net.BCrypt.Verify(
            senha,
            usuario.Senha
        );

        return senhaValida ? usuario : null;
    }
}

}
