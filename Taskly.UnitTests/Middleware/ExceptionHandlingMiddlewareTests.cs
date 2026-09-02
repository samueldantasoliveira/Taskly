using Microsoft.AspNetCore.Http;

namespace Taskly.Tests.Middleware;

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
            _ => throw new OperationCanceledException(cancellationTokenSource.Token));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_UnrelatedCancellation_ReturnsInternalServerError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new OperationCanceledException());

        await middleware.InvokeAsync(context);

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            context.Response.StatusCode);
    }
}
