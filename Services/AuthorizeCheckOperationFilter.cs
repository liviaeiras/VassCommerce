using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VassCommerce.Api.Services;

public sealed class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context
    )
    {
        var method = context.MethodInfo;
        var controller = method.DeclaringType;
        var allowsAnonymous =
            method.IsDefined(typeof(AllowAnonymousAttribute), true) ||
            controller?.IsDefined(
                typeof(AllowAnonymousAttribute),
                true
            ) == true;

        if (allowsAnonymous)
        {
            return;
        }

        var requiresAuthorization =
            method.IsDefined(typeof(AuthorizeAttribute), true) ||
            controller?.IsDefined(
                typeof(AuthorizeAttribute),
                true
            ) == true;

        if (!requiresAuthorization)
        {
            return;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = Array.Empty<string>()
            }
        ];
    }
}
