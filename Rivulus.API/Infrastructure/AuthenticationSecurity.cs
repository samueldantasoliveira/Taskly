using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Rivulus.Application;

namespace Rivulus.Infrastructure;

public sealed class AuthenticationLimits
{
    public int LoginPerMinute { get; set; } = 20;
    public int RegistrationPerMinute { get; set; } = 10;
}

public static class AuthenticationSecurity
{
    public static async Task ValidateSession(TokenValidatedContext context)
    {
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var version = context.Principal?.FindFirstValue("session_version");
        if (!Guid.TryParse(id, out var userId) || string.IsNullOrEmpty(version))
        {
            context.Fail("Invalid session.");
            return;
        }

        var repository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repository.GetByIdAsync(userId, context.HttpContext.RequestAborted);
        if (user == null || user.SessionVersion != version)
            context.Fail("Session revoked.");
    }

    public static void AddAuthenticationLimits(this IServiceCollection services)
    {
        services.AddOptions<AuthenticationLimits>().BindConfiguration("AuthenticationLimits")
            .Validate(l => l.LoginPerMinute > 0 && l.RegistrationPerMinute > 0,
                "Authentication limits must be positive.").ValidateOnStart();
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { message = "Too many attempts. Please try again later." }, cancellationToken);
            };
            foreach (var policy in new[] { "login", "registration" })
                options.AddPolicy(policy, context =>
                {
                    var limits = context.RequestServices.GetRequiredService<IOptions<AuthenticationLimits>>().Value;
                    var address = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(address, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = policy == "login" ? limits.LoginPerMinute : limits.RegistrationPerMinute,
                        Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true
                    });
                });
        });
    }
}
