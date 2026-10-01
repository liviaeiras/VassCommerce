using System.Text.Json;

namespace VassCommerce.Api.Middlewares;

public sealed class BusinessRuleException(string message)
    : InvalidOperationException(message)
{
}

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger
)
{
    private const string ProblemContentType =
        "application/problem+json";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var traceId = context.TraceIdentifier;

            logger.LogError(
                exception,
                "Erro não tratado na requisição {TraceId} para {Path}.",
                traceId,
                context.Request.Path
            );

            if (context.Response.HasStarted)
            {
                throw;
            }

            await WriteErrorResponseAsync(
                context,
                exception,
                traceId
            );
        }
    }

    private static Task WriteErrorResponseAsync(
        HttpContext context,
        Exception exception,
        string traceId
    )
    {
        var (status, title, detail, type) = exception switch
        {
            BusinessRuleException =>
                (
                    StatusCodes.Status400BadRequest,
                    "Operação inválida",
                    "A operação não pôde ser concluída porque os dados informados são inválidos.",
                    "https://httpstatuses.com/400"
                ),
            InvalidOperationException =>
                (
                    StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor",
                    "Não foi possível concluir a operação.",
                    "https://httpstatuses.com/500"
                ),
            _ =>
                (
                    StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor",
                    "Não foi possível concluir a operação.",
                    "https://httpstatuses.com/500"
                )
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = ProblemContentType;

        var problem = new
        {
            type,
            title,
            status,
            detail,
            instance = context.Request.Path.Value,
            traceId
        };

        return JsonSerializer.SerializeAsync(
            context.Response.Body,
            problem
        );
    }
}
