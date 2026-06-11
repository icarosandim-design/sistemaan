using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Common.Exceptions;

namespace SistemaAN.Api.Middleware;

/// <summary>
/// Tratamento global de erros. Converte exceções conhecidas da Aplicação em
/// respostas <c>ProblemDetails</c> (RFC 7807) com o status HTTP adequado e
/// registra as não tratadas como erro.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            ValidationException => (StatusCodes.Status400BadRequest, "Erro de validação"),
            BusinessRuleException => (StatusCodes.Status409Conflict, "Regra de negócio violada"),
            ForbiddenAccessException => (StatusCodes.Status403Forbidden, "Acesso negado"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor"),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("{Title}: {Message}", title, exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "Ocorreu um erro inesperado. Tente novamente mais tarde."
                : exception.Message,
            Instance = httpContext.Request.Path,
        };

        if (exception is ValidationException validation && validation.Errors.Count > 0)
        {
            problem.Extensions["errors"] = validation.Errors;
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
