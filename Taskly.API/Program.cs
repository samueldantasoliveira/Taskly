using MongoDB.Bson;
using MongoDB.Driver;
using Taskly.Application;
using Taskly.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Controller
builder.Services.AddControllers();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoClient
var mongoClient = new MongoClient("mongodb://localhost:27017");
builder.Services.AddSingleton(mongoClient);

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
app.UseAuthorization();

app.MapControllers();

app.Run();