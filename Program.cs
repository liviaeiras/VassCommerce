using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VassCommerce.Api.Data;
using VassCommerce.Api.Middlewares;
using VassCommerce.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Injeção de dependência dos serviços
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<EnderecoService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<PagamentoService>();
builder.Services.AddScoped<CartaoService>();

// Controllers e conversão de enums para texto no JSON
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors
                        .Select(error =>
                            string.IsNullOrWhiteSpace(
                                error.ErrorMessage
                            )
                                ? "O valor informado é inválido."
                                : error.ErrorMessage
                        )
                        .ToArray()
                );

            var response = new
            {
                type = "https://httpstatuses.com/400",
                title = "Erro de validação",
                status = StatusCodes.Status400BadRequest,
                detail =
                    "Um ou mais campos possuem valores inválidos.",
                errors,
                traceId = context.HttpContext.TraceIdentifier
            };

            return new BadRequestObjectResult(response)
            {
                ContentTypes =
                {
                    "application/problem+json"
                }
            };
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

// Configuração do JWT
var jwt = builder.Configuration.GetSection("Jwt");
JwtConfigurationValidator.Validate(jwt);

var jwtKey = jwt["Key"]!;
var jwtIssuer = jwt["Issuer"];
var jwtAudience = jwt["Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "VassCommerce API",
            Version = "v1"
        }
    );

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description =
                "Informe somente o token JWT, sem o prefixo Bearer.",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header
        }
    );

    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Aplica as migrations e executa o Seed
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.Migrate();
    AppDbContext.Seed(db, app.Configuration);
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Arquivos estáticos
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

// Autenticação antes da autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }