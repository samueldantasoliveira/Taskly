using Taskly.Application.DTOs;
using System.Net.Http.Json;
public class ProjectTestHelper
{
    private readonly HttpClient _client;

    public ProjectTestHelper(HttpClient client)
    {
        _client = client;
    }

    public async Task<ProjectResponseDto> CreateProjectAsync(
        Guid teamId,
        string? name = null)
    {
        var project = new
        {
            Name = name ?? $"Test Project {Guid.NewGuid()}",
            Description = "Project created for integration tests",
            TeamId = teamId
        };

        var response =
            await _client.PostAsJsonAsync("/api/project", project);

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<ProjectResponseDto>())!;
    }
}
