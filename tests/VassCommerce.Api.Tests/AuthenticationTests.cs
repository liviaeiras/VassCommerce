using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VassCommerce.Api.Data;
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

        var response = await RegisterAsync(client);
        using var body = await ReadJsonAsync(response);

        Assert.DoesNotContain(
            body.RootElement.EnumerateObject(),
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
        string password = "SenhaSegura123"
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
                Cpf = "111.222.333-44",
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
}
