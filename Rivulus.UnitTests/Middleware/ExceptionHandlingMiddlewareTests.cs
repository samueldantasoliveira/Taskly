using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Rivulus.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_RequestCanceled_DoesNotReturnInternalServerError()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var context = new DefaultHttpContext
        {
            RequestAborted = cancellationTokenSource.Token
        };
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new OperationCanceledException(cancellationTokenSource.Token), NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_UnrelatedCancellation_ReturnsInternalServerError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new OperationCanceledException(), NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            context.Response.StatusCode);
    }

    [Fact]
    public async Task UnexpectedFailure_LogsExceptionAndReturnsOnlyPublicMessageAndCorrelation()
    {
        var logger = new RecordingLogger();
        var failure = new InvalidOperationException("internal details");
        var context = new DefaultHttpContext { TraceIdentifier = "test-request" };
        context.Response.Body = new MemoryStream();
        await new ExceptionHandlingMiddleware(_ => throw failure, logger).InvokeAsync(context);
        context.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(500, context.Response.StatusCode);
        Assert.Equal("test-request", context.Response.Headers["X-Request-ID"]);
        Assert.Equal("test-request", body.RootElement.GetProperty("traceId").GetString());
        Assert.DoesNotContain("internal details", body.RootElement.ToString());
        Assert.Same(failure, logger.Exception);
        Assert.Contains("test-request", logger.Message);
    }

    private sealed class RecordingLogger : ILogger<ExceptionHandlingMiddleware>
    {
        public Exception? Exception { get; private set; }
        public string Message { get; private set; } = "";
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Exception = exception;
            Message = formatter(state, exception);
        }
    }
}
