using System.Diagnostics;
using System.Security.Claims;

namespace Taskly.Infrastructure;

public class RequestObservabilityMiddleware(RequestDelegate next, ILogger<RequestObservabilityMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context.Request.Headers[HeaderName].FirstOrDefault());
        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;
        var stopwatch = Stopwatch.StartNew();

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                logger.LogInformation(
                    "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds} ms for user {UserId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response.StatusCode,
                    stopwatch.Elapsed.TotalMilliseconds,
                    userId ?? "anonymous");
            }
        }
    }

    private static string ResolveCorrelationId(string? supplied) =>
        Guid.TryParse(supplied, out var parsed) ? parsed.ToString("D") : Guid.NewGuid().ToString("D");
}
