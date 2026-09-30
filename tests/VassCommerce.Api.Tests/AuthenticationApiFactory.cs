using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Cryptography;
using VassCommerce.Api.Data;

namespace VassCommerce.Api.Tests;

public sealed class AuthenticationApiFactory : WebApplicationFactory<Program>
{
    private static readonly string TestJwtKey =
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private readonly string _databaseName =
        $"VassCommerceTests-{Guid.NewGuid():N}";

    static AuthenticationApiFactory()
    {
        Environment.SetEnvironmentVariable(
            "Jwt__Key",
            TestJwtKey
        );
        Environment.SetEnvironmentVariable(
            "Jwt__Issuer",
            "VassCommerce.Tests"
        );
        Environment.SetEnvironmentVariable(
            "Jwt__Audience",
            "VassCommerce.Tests.Client"
        );
        Environment.SetEnvironmentVariable(
            "Jwt__ExpiresMinutes",
            "15"
        );
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder
    )
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options
                    .UseInMemoryDatabase(_databaseName)
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(
                            InMemoryEventId.TransactionIgnoredWarning
                        )
                    )
            );
        });
    }
}
