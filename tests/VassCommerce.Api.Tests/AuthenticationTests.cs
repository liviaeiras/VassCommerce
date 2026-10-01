using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using VassCommerce.Api.Data;
using VassCommerce.Api.Models;
using VassCommerce.Api.Services;
using Xunit;

namespace VassCommerce.Api.Tests;

public sealed class AuthenticationTests
{
    [Fact]
    public async Task Register_WithValidData_ReturnsCreatedCustomer()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();

        var response = await RegisterAsync(client);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var body = await ReadJsonAsync(response);
        Assert.Equal("cliente@example.com", body.RootElement
            .GetProperty("email").GetString());
        Assert.Equal("Cliente", body.RootElement
            .GetProperty("perfil").GetString());
        Assert.Equal(1, body.RootElement
            .GetProperty("clienteId").GetInt32());

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(await db.Administradores.ToListAsync());
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();
        await RegisterAsync(client);

        var response = await RegisterAsync(
            client,
            email: "  CLIENTE@example.com "
        );

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();

        var response = await RegisterAsync(
            client,
            email: "email-invalido"
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithShortPassword_ReturnsBadRequest()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();

        var response = await RegisterAsync(
            client,
            password: "curta"
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_StoresPasswordHashInsteadOfPlainText()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();
        await RegisterAsync(client);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Usuarios.SingleAsync();

        Assert.NotEqual("SenhaSegura123", user.Senha);
        Assert.True(BCrypt.Net.BCrypt.Verify(
            "SenhaSegura123",
            user.Senha
        ));
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();
        await RegisterAsync(client);

        var response = await client.PostAsJsonAsync(
            "/auth/login",
            new
            {
                Email = "  CLIENTE@example.com ",
                Senha = "SenhaSegura123"
            }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = await ReadJsonAsync(response);
        Assert.False(string.IsNullOrWhiteSpace(
            body.RootElement.GetProperty("token").GetString()
        ));
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsGenericUnauthorized()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();

        var response = await LoginAsync(
            client,
            "inexistente@example.com",
            "SenhaSegura123"
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(
            "E-mail ou senha inválidos.",
            await GetMessageAsync(response)
        );
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsSameUnauthorizedMessage()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();
        await RegisterAsync(client);

        var response = await LoginAsync(
            client,
            "cliente@example.com",
            "SenhaIncorreta123"
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(
            "E-mail ou senha inválidos.",
            await GetMessageAsync(response)
        );
    }

    [Fact]
    public async Task Login_WithPasswordLongerThanBcryptLimit_ReturnsUnauthorized()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();
        await RegisterAsync(client);

        var response = await LoginAsync(
            client,
            "cliente@example.com",
            new string('a', 73)
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(
            "E-mail ou senha inválidos.",
            await GetMessageAsync(response)
        );
    }

    [Fact]
    public async Task Login_TokenContainsRequiredClaimsAndAuthenticates()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();
        await RegisterAsync(client);

        var response = await LoginAsync(
            client,
            "cliente@example.com",
            "SenhaSegura123"
        );
        using var body = await ReadJsonAsync(response);
        var token = body.RootElement.GetProperty("token").GetString()!;
        var expiresAt = body.RootElement
            .GetProperty("expiraEm")
            .GetDateTime();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.InRange(
            (expiresAt - DateTime.UtcNow).TotalMinutes,
            14,
            15
        );
        Assert.Contains(
            jwt.Claims,
            claim =>
                claim.Type == ClaimTypes.NameIdentifier &&
                claim.Value == "1"
        );
        Assert.Contains(
            jwt.Claims,
            claim =>
                claim.Type == ClaimTypes.Email &&
                claim.Value == "cliente@example.com"
        );
        Assert.Contains(
            jwt.Claims,
            claim =>
                claim.Type == ClaimTypes.Role &&
                claim.Value == "Cliente"
        );

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        var protectedResponse = await client.GetAsync("/auth/perfil");
        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
    }

    [Fact]
    public async Task Customer_IsForbiddenFromAdministrativeEndpoint()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();
        var registration = await RegisterAsync(client);
        using var body = await ReadJsonAsync(registration);
        var token = body.RootElement.GetProperty("token").GetString()!;

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync(
            "/auth/area-administrativa"
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Customer_CannotReadAnotherCustomersPrivateData()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();
        var firstRegistration = await RegisterAsync(client);
        using var firstBody = await ReadJsonAsync(firstRegistration);
        var token = firstBody.RootElement
            .GetProperty("token")
            .GetString()!;

        var secondRegistration = await RegisterAsync(
            client,
            email: "outro@example.com",
            cpf: "555.666.777-88"
        );
        using var secondBody = await ReadJsonAsync(secondRegistration);
        var otherCustomerId = secondBody.RootElement
            .GetProperty("clienteId")
            .GetInt32();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var customerResponse = await client.GetAsync(
            $"/cliente/{otherCustomerId}"
        );
        var cardsResponse = await client.GetAsync(
            $"/cliente/{otherCustomerId}/formas-de-pagamento"
        );
        var addressResponse = await client.GetAsync(
            $"/cliente/{otherCustomerId}/endereco"
        );

        Assert.Equal(HttpStatusCode.NotFound, customerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, cardsResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, addressResponse.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutTokenReturnsUnauthorized()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/cliente/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationResponse_DoesNotExposePasswordOrHash()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();

        var registration = await RegisterAsync(client);
        using var registrationBody = await ReadJsonAsync(registration);
        AssertNoPasswordOrHash(registrationBody.RootElement);

        var login = await LoginAsync(
            client,
            "cliente@example.com",
            "SenhaSegura123"
        );
        using var loginBody = await ReadJsonAsync(login);
        AssertNoPasswordOrHash(loginBody.RootElement);
    }

    [Fact]
    public void JwtConfiguration_WithoutKeyFailsClearly()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "VassCommerce.Tests",
                    ["Jwt:Audience"] =
                        "VassCommerce.Tests.Client",
                    ["Jwt:ExpiresMinutes"] = "15"
                }
            )
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () => JwtConfigurationValidator.Validate(
                configuration.GetSection("Jwt")
            )
        );

        Assert.Contains("Jwt:Key", exception.Message);
        Assert.Contains("User Secrets", exception.Message);
    }

    [Fact]
    public async Task CatalogReadEndpoints_RemainPublic()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();
            var categoryRecord = new Categoria
            {
                Nome = "Categoria pública",
                Descricao = "Consulta pública",
                ImagemSimboloUrl = string.Empty
            };
            db.Categorias.Add(categoryRecord);
            await db.SaveChangesAsync();
            db.Produtos.Add(new Produto
            {
                Nome = "Produto público",
                Descricao = "Consulta pública",
                FotoUrl = string.Empty,
                CategoriaId = categoryRecord.Id,
                DataCadastro = DateTime.UtcNow,
                DataUltimaAtualizacao = DateTime.UtcNow,
                ValorUnitario = 10m
            });
            await db.SaveChangesAsync();
        }

        var categories = await client.GetAsync("/categoria");
        var category = await client.GetAsync("/categoria/1");
        var products = await client.GetAsync("/categoria/1/produto");
        var product = await client.GetAsync("/produto/1");

        Assert.Equal(HttpStatusCode.OK, categories.StatusCode);
        Assert.Equal(HttpStatusCode.OK, category.StatusCode);
        Assert.Equal(HttpStatusCode.OK, products.StatusCode);
        Assert.Equal(HttpStatusCode.OK, product.StatusCode);
    }

    [Fact]
    public void Seed_WithoutAdminConfigurationDoesNotCreateAdministrator()
    {
        using var db = CreateSeedDatabase();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        AppDbContext.Seed(db, configuration);

        Assert.Empty(db.Administradores);
        Assert.DoesNotContain(
            db.Usuarios,
            usuario => usuario.Email == "admin@vasscommerce.com"
        );
    }

    [Fact]
    public void Seed_WithExplicitAdminConfigurationIsIdempotent()
    {
        const string email = "admin.tests@example.com";
        const string password = "SomenteParaTeste123!";
        using var db = CreateSeedDatabase();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Admin:Email"] = email,
                    ["Admin:Password"] = password
                }
            )
            .Build();

        AppDbContext.Seed(db, configuration);
        AppDbContext.Seed(db, configuration);

        var administrator = Assert.Single(db.Administradores);
        var user = Assert.Single(db.Usuarios);
        Assert.Equal(email, user.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify(password, user.Senha));
        Assert.Equal(user.Id, administrator.UsuarioId);
    }

    [Fact]
    public async Task TamperedJwt_IsRejectedByProtectedEndpoint()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();
        var registration = await RegisterAsync(client);
        using var body = await ReadJsonAsync(registration);
        var token = body.RootElement.GetProperty("token").GetString()!;
        var segments = token.Split('.');
        var firstSignatureCharacter = segments[2][0];
        segments[2] = (firstSignatureCharacter == 'A' ? "B" : "A") +
            segments[2][1..];

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                string.Join('.', segments)
            );
        var response = await client.GetAsync("/auth/perfil");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("issuer")]
    [InlineData("audience")]
    public async Task Jwt_WithInvalidLifetimeIssuerOrAudience_IsRejected(
        string invalidClaim
    )
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();
        var now = DateTime.UtcNow;
        var token = CreateSignedToken(
            issuer: invalidClaim == "issuer"
                ? "VassCommerce.Invalid"
                : "VassCommerce.Tests",
            audience: invalidClaim == "audience"
                ? "VassCommerce.Invalid.Client"
                : "VassCommerce.Tests.Client",
            expires: invalidClaim == "expired"
                ? now.AddMinutes(-1)
                : now.AddMinutes(15)
        );
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/auth/perfil");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_RequiresBearerOnlyForProtectedEndpoints()
    {
        using var factory = new AuthenticationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var swagger = await ReadJsonAsync(response);
        var document = swagger.RootElement;
        Assert.True(document.GetProperty("components")
            .GetProperty("securitySchemes")
            .TryGetProperty("Bearer", out _));
        Assert.False(document.GetProperty("paths")
            .GetProperty("/produto/{id}")
            .GetProperty("get")
            .TryGetProperty("security", out _));
        Assert.True(document.GetProperty("paths")
            .GetProperty("/auth/area-administrativa")
            .GetProperty("get")
            .TryGetProperty("security", out _));
    }

    private static Task<HttpResponseMessage> RegisterAsync(
        HttpClient client,
        string email = " cliente@example.com ",
        string password = "SenhaSegura123",
        string cpf = "111.222.333-44"
    )
    {
        return client.PostAsJsonAsync(
            "/auth/register",
            new
            {
                NomeCompleto = "Cliente Teste",
                Email = email,
                Senha = password,
                DataNascimento = new DateTime(1995, 4, 12),
                Cpf = cpf,
                Perfil = "Administrador"
            }
        );
    }

    private static Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string email,
        string password
    )
    {
        return client.PostAsJsonAsync(
            "/auth/login",
            new
            {
                Email = email,
                Senha = password
            }
        );
    }

    private static async Task<JsonDocument> ReadJsonAsync(
        HttpResponseMessage response
    )
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }

    private static async Task<string?> GetMessageAsync(
        HttpResponseMessage response
    )
    {
        using var body = await ReadJsonAsync(response);
        return body.RootElement.GetProperty("mensagem").GetString();
    }

    private static void AssertNoPasswordOrHash(JsonElement response)
    {
        Assert.DoesNotContain(
            response.EnumerateObject(),
            property =>
                property.Name.Contains(
                    "senha",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                property.Name.Contains(
                    "hash",
                    StringComparison.OrdinalIgnoreCase
                )
        );
    }

    private static AppDbContext CreateSeedDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"SeedTests-{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options);
    }

    private static string CreateSignedToken(
        string issuer,
        string audience,
        DateTime expires
    )
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                AuthenticationApiFactory.JwtSigningKey
            )
        );
        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256
        );
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "cliente@example.com"),
            new Claim(ClaimTypes.Role, "Cliente")
        };
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
