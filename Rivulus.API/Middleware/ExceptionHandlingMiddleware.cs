using Rivulus.Domain.Exceptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        context.Response.Headers["X-Request-ID"] = traceId;
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = traceId });
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected or canceled the request. No response is needed.
        }
        catch (InvalidTaskTransitionException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(exception.Message, context.RequestAborted);
        }
        catch (InvalidTaskAssignmentException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(exception.Message, context.RequestAborted);
        }
        catch (InvalidTaskCompletionException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(exception.Message, context.RequestAborted);
        }
        catch (InvalidTaskUpdateException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(exception.Message, context.RequestAborted);
        }
        catch (UserAlreadyMemberException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(exception.Message, context.RequestAborted);
        }
        catch (UserNotMemberException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(exception.Message, context.RequestAborted);
        }
        catch (OwnerCannotBeRemovedException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(exception.Message, context.RequestAborted);
        }
        catch (InvalidUserNameException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(exception.Message, context.RequestAborted);
        }
        catch (InvalidUserEmailException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(exception.Message, context.RequestAborted);
        }
        catch (InvalidUserPasswordException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(exception.Message, context.RequestAborted);
        }
        catch (Rivulus.Application.Results.PendingResponsibilitiesException exception)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new { message = exception.Message }, context.RequestAborted);
        }
        catch (Rivulus.Application.Results.ConcurrencyConflictException exception)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new { message = exception.Message }, context.RequestAborted);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled request failure. RequestId: {RequestId}", traceId);
            if (context.Response.HasStarted) throw;
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred.", traceId }, context.RequestAborted);
        }
    }
}
