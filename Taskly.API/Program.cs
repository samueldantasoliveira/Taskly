using MongoDB.Bson;
using MongoDB.Driver;
using Taskly.Application;
using Taskly.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// JWT CONFIG
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
builder.Services.AddSwaggerGen();

// MongoClient
var mongoClient = new MongoClient("mongodb://localhost:27017");
builder.Services.AddSingleton(mongoClient);

// Token Service
builder.Services.AddSingleton<ITokenService, TokenService>();

// DI
builder.Services.AddScoped<MongoDbContext>();
builder.Services.AddScoped<TodoTaskService>();
builder.Services.AddScoped<ITodoTaskRepository, TodoTaskRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<LoginService>();

var app = builder.Build();

//Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();