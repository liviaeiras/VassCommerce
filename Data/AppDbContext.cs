using Microsoft.EntityFrameworkCore;
using VassCommerce.Api.Models;

namespace VassCommerce.Api.Data;

public class AppDbContext(
    DbContextOptions<AppDbContext> options
) : DbContext(options)
{
    public DbSet<Estado> Estados => Set<Estado>();
    public DbSet<Cidade> Cidades => Set<Cidade>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Produto> Produtos => Set<Produto>();

    public DbSet<TabelaPromocao> TabelasPromocao =>
        Set<TabelaPromocao>();

    public DbSet<PromocaoItem> PromocaoItens =>
        Set<PromocaoItem>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Administrador> Administradores =>
        Set<Administrador>();

    public DbSet<Cartao> Cartoes => Set<Cartao>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder
    )
    {
        base.OnModelCreating(modelBuilder);

        // E-mail único
        modelBuilder.Entity<Usuario>()
            .HasIndex(usuario => usuario.Email)
            .IsUnique();

        // CPF único
        modelBuilder.Entity<Cliente>()
            .HasIndex(cliente => cliente.Cpf)
            .IsUnique();

        // Valores monetários
        modelBuilder.Entity<Produto>()
            .Property(produto => produto.ValorUnitario)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PromocaoItem>()
            .Property(item => item.ValorPromocao)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Pedido>()
            .Property(pedido => pedido.ValorTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ItemPedido>()
            .Property(item => item.ValorUnitario)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Pagamento>()
            .Property(pagamento => pagamento.ValorPago)
            .HasPrecision(18, 2);

        // Uma categoria possui vários produtos.
        modelBuilder.Entity<Produto>()
            .HasOne(produto => produto.Categoria)
            .WithMany(categoria => categoria.Produtos)
            .HasForeignKey(produto => produto.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Um usuário possui, no máximo, um cliente.
        modelBuilder.Entity<Cliente>()
            .HasOne(cliente => cliente.Usuario)
            .WithOne(usuario => usuario.Cliente)
            .HasForeignKey<Cliente>(
                cliente => cliente.UsuarioId
            )
            .OnDelete(DeleteBehavior.Cascade);

        // Um usuário possui, no máximo, um administrador.
        modelBuilder.Entity<Administrador>()
            .HasOne(administrador => administrador.Usuario)
            .WithOne(usuario => usuario.Administrador)
            .HasForeignKey<Administrador>(
                administrador => administrador.UsuarioId
            )
            .OnDelete(DeleteBehavior.Cascade);

        // Uma categoria possui, no máximo,
        // um administrador responsável.
        modelBuilder.Entity<Administrador>()
            .HasOne(administrador => administrador.Categoria)
            .WithOne(categoria => categoria.Administrador)
            .HasForeignKey<Administrador>(
                administrador => administrador.CategoriaId
            )
            .OnDelete(DeleteBehavior.Restrict);
    }

    public static void Seed(AppDbContext db)
    {
        var agora = DateTime.UtcNow;

        // Garante que a categoria Informática exista.
        var informatica = db.Categorias.FirstOrDefault(
            categoria => categoria.Nome == "Informática"
        );

        if (informatica is null)
        {
            informatica = new Categoria
            {
                Nome = "Informática",
                Descricao =
                    "Computadores, periféricos e acessórios",
                ImagemSimboloUrl =
                    "/img/categorias/informatica.png"
            };

            db.Categorias.Add(informatica);
            db.SaveChanges();
        }

        // Garante que a categoria Livros exista.
        var livros = db.Categorias.FirstOrDefault(
            categoria => categoria.Nome == "Livros"
        );

        if (livros is null)
        {
            livros = new Categoria
            {
                Nome = "Livros",
                Descricao = "Livros em geral",
                ImagemSimboloUrl =
                    "/img/categorias/livros.png"
            };

            db.Categorias.Add(livros);
            db.SaveChanges();
        }

        // Adiciona somente os produtos que ainda não existem.
        if (!db.Produtos.Any(
                produto => produto.Nome == "Notebook 15\""
            ))
        {
            db.Produtos.Add(new Produto
            {
                Nome = "Notebook 15\"",
                Descricao =
                    "Notebook 15 polegadas, 16GB RAM, SSD 512GB",
                FotoUrl = "/img/produtos/1.png",
                ValorUnitario = 4500.00m,
                CategoriaId = informatica.Id,
                DataCadastro = agora,
                DataUltimaAtualizacao = agora
            });
        }

        if (!db.Produtos.Any(
                produto => produto.Nome == "Mouse sem fio"
            ))
        {
            db.Produtos.Add(new Produto
            {
                Nome = "Mouse sem fio",
                Descricao = "Mouse óptico sem fio",
                FotoUrl = "/img/produtos/2.png",
                ValorUnitario = 89.90m,
                CategoriaId = informatica.Id,
                DataCadastro = agora,
                DataUltimaAtualizacao = agora
            });
        }

        if (!db.Produtos.Any(
                produto => produto.Nome == "Teclado"
            ))
        {
            db.Produtos.Add(new Produto
            {
                Nome = "Teclado",
                Descricao = "Teclado mecânico",
                FotoUrl = "/img/produtos/4.png",
                ValorUnitario = 150.00m,
                CategoriaId = informatica.Id,
                DataCadastro = agora,
                DataUltimaAtualizacao = agora
            });
        }

        if (!db.Produtos.Any(
                produto => produto.Nome == "Clean Code"
            ))
        {
            db.Produtos.Add(new Produto
            {
                Nome = "Clean Code",
                Descricao =
                    "Livro sobre boas práticas de programação",
                FotoUrl = "/img/produtos/3.png",
                ValorUnitario = 120.00m,
                CategoriaId = livros.Id,
                DataCadastro = agora,
                DataUltimaAtualizacao = agora
            });
        }
        // Garante que o estado do Rio de Janeiro exista.
        var rioDeJaneiro = db.Estados.FirstOrDefault(
    estado => estado.Sigla == "RJ"
);

          if (rioDeJaneiro is null)
{
              rioDeJaneiro = new Estado
    {
        Nome = "Rio de Janeiro",
        Sigla = "RJ"
    };

    db.Estados.Add(rioDeJaneiro);
    db.SaveChanges();
}

// Garante que a cidade de Barra Mansa exista.
var barraMansaExiste = db.Cidades.Any(
    cidade =>
        cidade.Nome == "Barra Mansa" &&
        cidade.EstadoId == rioDeJaneiro.Id
);

if (!barraMansaExiste)
{
    var barraMansa = new Cidade
    {
        Nome = "Barra Mansa",
        EstadoId = rioDeJaneiro.Id
    };

    db.Cidades.Add(barraMansa);
    db.SaveChanges();
}

        db.SaveChanges();

        CriarAdministradorDesenvolvimento(
            db,
            informatica,
            agora
        );
    }

    private static void CriarAdministradorDesenvolvimento(
        AppDbContext db,
        Categoria categoria,
        DateTime agora
    )
    {
        const string emailAdministrador =
            "admin@vasscommerce.com";

        var usuarioAdministrador = db.Usuarios.FirstOrDefault(
            usuario => usuario.Email == emailAdministrador
        );

        // Cria o usuário administrativo caso não exista.
        if (usuarioAdministrador is null)
        {
            usuarioAdministrador = new Usuario
            {
                NomeCompleto =
                    "Administrador VassCommerce",
                Email = emailAdministrador,
                Senha = BCrypt.Net.BCrypt.HashPassword(
                    "Admin@12345"
                ),
                FotoUrl = string.Empty,
                DataCadastro = agora,
                DataUltimaAtualizacao = agora
            };

            db.Usuarios.Add(usuarioAdministrador);
            db.SaveChanges();
        }

        var administradorExiste =
            db.Administradores.Any(
                administrador =>
                    administrador.UsuarioId ==
                    usuarioAdministrador.Id
            );

        var categoriaJaPossuiAdministrador =
            db.Administradores.Any(
                administrador =>
                    administrador.CategoriaId ==
                    categoria.Id
            );

        if (administradorExiste ||
            categoriaJaPossuiAdministrador)
        {
            return;
        }

        var administrador = new Administrador
        {
            UsuarioId = usuarioAdministrador.Id,
            CategoriaId = categoria.Id
        };

        db.Administradores.Add(administrador);
        db.SaveChanges();
    }
}
