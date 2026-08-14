using Taskly.Application.DTOs;
using System.Net.Http.Json;
using System.Net.Http.Headers;
public class ProjectTestHelper
{
    private readonly HttpClient _client;

    public ProjectTestHelper(HttpClient client)
    {
        _client = client;
    }

    public async Task<ProjectResponseDto> CreateProjectAsync(Guid teamId)
    {
        var project = new
        {
            Name = "Test Project",
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