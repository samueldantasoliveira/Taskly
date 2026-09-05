using MongoDB.Bson;
using MongoDB.Driver;
using Taskly.Application;
using Taskly.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

var builder = WebApplication.CreateBuilder(args);

// HOST FILTERING CONFIG
var allowedHostsOptionsBuilder = builder.Services
    .AddOptions<AllowedHostsSettings>()
    .Configure(settings =>
    {
        settings.Value = builder.Configuration["AllowedHosts"] ?? "";
    })
    .Validate(
        settings => HasAllowedHosts(settings.Value),
        "AllowedHosts must contain at least one host."
    );

if (builder.Environment.IsProduction())
{
    allowedHostsOptionsBuilder.Validate(
        settings => !ContainsTopLevelWildcard(settings.Value),
        "AllowedHosts cannot contain '*' in Production."
    );
}

allowedHostsOptionsBuilder.ValidateOnStart();

// CORS CONFIG
const string corsPolicyName = "Frontend";
builder.Services
    .AddOptions<CorsSettings>()
    .BindConfiguration(CorsSettings.SectionName)
    .Validate(
        settings => settings.AllowedOrigins.Length > 0,
        "Cors:AllowedOrigins must contain at least one origin."
    )
    .Validate(
        settings => settings.AllowedOrigins.All(IsValidOrigin),
        "Cors:AllowedOrigins must contain only valid HTTP or HTTPS origins."
    )
    .ValidateOnStart();

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    options.AddPolicy(corsPolicyName, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// JWT CONFIG
var jwtOptionsBuilder = builder.Services
    .AddOptions<JwtSettings>()
    .BindConfiguration(JwtSettings.SectionName)
    .Validate(
        settings => IsValidJwtKey(settings.Key),
        "Jwt:Key must be a valid Base64 key containing at least 32 bytes."
    )
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.Issuer),
        "Jwt:Issuer is required."
    )
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.Audience),
        "Jwt:Audience is required."
    )
    .Validate(
        settings => settings.ExpiresMinutes > 0,
        "Jwt:ExpiresMinutes must be greater than zero."
    );

if (builder.Environment.IsProduction())
{
    jwtOptionsBuilder.Validate(
        settings => settings.Key != JwtSettings.DevelopmentKey,
        "The development JWT key cannot be used in Production."
    );
}

jwtOptionsBuilder.ValidateOnStart();

var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(jwtSettings["Key"]!)
            )
        };
    });

// Controller
builder.Services.AddControllers();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Taskly API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta forma: Bearer {seu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});


// MongoClient
if (BsonSerializer.LookupSerializer<Guid>() is not GuidSerializer)
{
    BsonSerializer.RegisterSerializer(
        new GuidSerializer(GuidRepresentation.Standard)
    );
}

builder.Services
    .AddOptions<MongoDbSettings>()
    .BindConfiguration(MongoDbSettings.SectionName)
    .Validate(
        settings => IsValidMongoConnectionString(settings.ConnectionString),
        "MongoDb:ConnectionString must be a valid MongoDB connection string."
    )
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.DatabaseName),
        "MongoDb:DatabaseName is required."
    )
    .ValidateOnStart();

builder.Services.AddSingleton(serviceProvider =>
{
    var mongoDbSettings = serviceProvider
        .GetRequiredService<IOptions<MongoDbSettings>>()
        .Value;

    return new MongoClient(mongoDbSettings.ConnectionString);
});

// Token Service
builder.Services.AddSingleton<ITokenService, TokenService>();

// DI
builder.Services.AddScoped<MongoDbContext>();
builder.Services.AddScoped<TodoTaskService>();
builder.Services.AddScoped<ITodoTaskRepository, TodoTaskRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<LoginService>();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        await context.EnsureIndexesAsync(app.Lifetime.ApplicationStopping);
    }  
}


//Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok());

app.Run();

static bool IsValidOrigin(string origin)
{
    if (string.IsNullOrWhiteSpace(origin)
        || origin.EndsWith('/'))
    {
        return false;
    }

    return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp
            || uri.Scheme == Uri.UriSchemeHttps)
        && string.IsNullOrEmpty(uri.UserInfo)
        && uri.AbsolutePath == "/"
        && string.IsNullOrEmpty(uri.Query)
        && string.IsNullOrEmpty(uri.Fragment);
}

static bool HasAllowedHosts(string allowedHosts)
{
    return allowedHosts
        .Split(
            ';',
            StringSplitOptions.RemoveEmptyEntries
            | StringSplitOptions.TrimEntries
        )
        .Length > 0;
}

static bool ContainsTopLevelWildcard(string allowedHosts)
{
    return allowedHosts
        .Split(
            ';',
            StringSplitOptions.RemoveEmptyEntries
            | StringSplitOptions.TrimEntries
        )
        .Contains("*", StringComparer.Ordinal);
}

static bool IsValidJwtKey(string key)
{
    if (string.IsNullOrWhiteSpace(key))
    {
        return false;
    }

    try
    {
        return Convert.FromBase64String(key).Length >= 32;
    }
    catch (FormatException)
    {
        return false;
    }
}

static bool IsValidMongoConnectionString(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return false;
    }

    try
    {
        _ = new MongoUrl(connectionString);
        return true;
    }
    catch (Exception exception) when (
        exception is FormatException
        or MongoConfigurationException
    )
    {
        return false;
    }
}

public partial class Program
{
}

public sealed class CorsSettings
{
    public const string SectionName = "Cors";
    public string[] AllowedOrigins { get; init; } = [];
}

public sealed class AllowedHostsSettings
{
    public string Value { get; set; } = "";
}

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public const string DevelopmentKey =
        "Ay3/YsgUxPYBiJ+BXFfC50wEUy7/uBk2swF+0uBgwGA=";

    public string Key { get; init; } = "";
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public int ExpiresMinutes { get; init; }
}

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; init; } = "";
    public string DatabaseName { get; init; } = "";
}
