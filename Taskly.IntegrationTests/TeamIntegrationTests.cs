using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Taskly.Application.DTOs;
using Taskly.Domain.Entities;

namespace Taskly.IntegrationTests;

public class TeamIntegrationTests : IClassFixture<TasklyApiFactory>
{
    private readonly HttpClient _client;
    private readonly UserTestHelper _userHelper;
    private readonly TeamTestHelper _teamHelper;
    private readonly TasklyApiFactory _factory;

    public TeamIntegrationTests(TasklyApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _userHelper = new UserTestHelper(_client);
        _teamHelper = new TeamTestHelper(_client);
    }

    [Fact]
    public async Task GetUserTeams_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/team");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTeamById_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync($"/api/team/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserTeams_ReturnsOnlyAuthenticatedUsersNonDeletedTeams()
    {
        var firstLogin = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(firstLogin.Token);

        var visibleTeam = await _teamHelper.CreateTeamAsync();
        var deletedTeam = await _teamHelper.CreateTeamAsync();

        var deleteResponse = await _client.DeleteAsync($"/api/team/{deletedTeam.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var secondLogin = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(secondLogin.Token);
        await _teamHelper.CreateTeamAsync();

        SetBearerToken(firstLogin.Token);

        var response = await _client.GetAsync("/api/team");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var teams = await response.Content.ReadFromJsonAsync<List<TeamResponseDto>>();

        Assert.NotNull(teams);
        var team = Assert.Single(teams);
        Assert.Equal(visibleTeam.Id, team.Id);
        Assert.Contains(firstLogin.User.Id, team.UserIds);
        Assert.DoesNotContain(teams, item => item.Id == deletedTeam.Id);
    }

    [Fact]
    public async Task GetTeamById_MemberReturnsTeamAndNonMemberIsForbidden()
    {
        var memberLogin = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(memberLogin.Token);
        var team = await _teamHelper.CreateTeamAsync();

        var memberResponse = await _client.GetAsync($"/api/team/{team.Id}");

        Assert.Equal(HttpStatusCode.OK, memberResponse.StatusCode);
        var returnedTeam = await memberResponse.Content.ReadFromJsonAsync<TeamResponseDto>();
        Assert.NotNull(returnedTeam);
        Assert.Equal(team.Id, returnedTeam.Id);

        var nonMemberLogin = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(nonMemberLogin.Token);

        var nonMemberResponse = await _client.GetAsync($"/api/team/{team.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, nonMemberResponse.StatusCode);
    }

    [Fact]
    public async Task GetTeamById_TeamNotFound_ReturnsNotFound()
    {
        var login = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(login.Token);

        var response = await _client.GetAsync($"/api/team/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTeamById_LegacyDocument_CanBeDeserialized()
    {
        var login = await _userHelper.CreateUserAndLoginAsync();
        SetBearerToken(login.Token);

        var teamId = Guid.NewGuid();
        var mongoClient = _factory.Services.GetRequiredService<MongoClient>();
        var database = mongoClient.GetDatabase("TasklyIntegrationTests");
        var teams = database.GetCollection<BsonDocument>("Teams");

        await teams.InsertOneAsync(new BsonDocument
        {
            ["_id"] = teamId.ToString(),
            ["Name"] = "Legacy team",
            ["IsActive"] = true,
            ["UserIds"] = new BsonArray { login.User.Id.ToString() }
        });

        var typedTeams = database.GetCollection<Team>("Teams");
        var deserializedTeam = await typedTeams
            .Find(team => team.Id == teamId)
            .FirstOrDefaultAsync();
        Assert.NotNull(deserializedTeam);

        var response = await _client.GetAsync($"/api/team/{teamId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var team = await response.Content.ReadFromJsonAsync<TeamResponseDto>();
        Assert.NotNull(team);
        Assert.Equal(teamId, team.Id);
        Assert.Equal("Legacy team", team.Name);
    }

    private void SetBearerToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
