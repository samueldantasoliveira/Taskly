using Taskly.Application.DTOs;
using System.Net.Http.Json;
public class TeamTestHelper
{
    private readonly HttpClient _client;

    public TeamTestHelper(HttpClient client)
    {
        _client = client;
    }

    public async Task<TeamResponseDto> CreateTeamAsync()
    {
        var team = new
        {
            Name = $"Test Team {Guid.NewGuid()}"
        };

        var response =
            await _client.PostAsJsonAsync("/api/team", team);

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<TeamResponseDto>())!;
    }
}