using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PartnerCommissions.Commissions.Api.Domain;

namespace PartnerCommissions.Commissions.Api.Hosting;

internal sealed class DomainExceptionHandler(
    ILogger<DomainExceptionHandler> logger,
    IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException domain)
        {
            return false;
        }

        logger.DomainError(domain, domain.ErrorCode, domain.StatusCode, domain.FormatDetails());

        httpContext.Response.StatusCode = domain.StatusCode;

        var details = new ProblemDetails
        {
            Title = domain.ErrorCode,
            Detail = domain.Message,
            Status = domain.StatusCode
        };

        foreach (var (key, value) in domain.Properties)
        {
            details.Extensions[key] = value;
        }

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = details,
            Exception = domain
        });
    }
}
