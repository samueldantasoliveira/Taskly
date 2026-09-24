using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Rivulus.Application;
using Rivulus.Application.DTOs;
using Rivulus.Application.Results;
using Rivulus.Domain;
using Rivulus.Domain.Entities;
using Rivulus.Infrastructure;

namespace Rivulus.IntegrationTests;

public class ConcurrencyIntegrationTests(RivulusApiFactory factory) : IClassFixture<RivulusApiFactory>
{
    [Fact]
    public async Task ConcurrentMemberChanges_DoNotOverwriteEachOther()
    {
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = new Team("Concurrent team", Guid.NewGuid());
        await repository.AddAsync(team);
        var first = (await repository.GetByIdAsync(team.Id))!;
        var second = (await repository.GetByIdAsync(team.Id))!;
        var firstMember = Guid.NewGuid();
        first.AddMember(firstMember);
        second.AddMember(Guid.NewGuid());
        Assert.True(await repository.UpdateAsync(first));
        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.UpdateAsync(second));
        var stored = (await repository.GetByIdAsync(team.Id))!;
        Assert.Contains(firstMember, stored.UserIds);
        Assert.Equal(2, stored.UserIds.Count);
    }

    [Fact]
    public async Task TaskEdit_CannotRevertConcurrentStatusChange()
    {
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITodoTaskRepository>();
        var task = new TodoTask("Original", "Description", Guid.NewGuid(), Guid.NewGuid());
        await repository.AddAsync(task);
        var first = (await repository.GetByIdAsync(task.Id))!;
        var second = (await repository.GetByIdAsync(task.Id))!;
        first.Start();
        second.Update("Stale edit", null);
        Assert.True(await repository.UpdateAsync(first));
        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.UpdateAsync(second));
        var stored = (await repository.GetByIdAsync(task.Id))!;
        Assert.Equal(TodoStatus.InProgress, stored.Status);
        Assert.Equal("Original", stored.Title);
    }

    [Fact]
    public async Task StaleUserEdit_DoesNotRestoreOldPasswordOrSession()
    {
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = new User("User", $"{Guid.NewGuid()}@test.com", "old-hash");
        await repository.AddAsync(user);
        var first = (await repository.GetByIdAsync(user.Id))!;
        var second = (await repository.GetByIdAsync(user.Id))!;
        first.Update(null, null, "new-hash");
        second.Update("Stale name", null, null);
        Assert.True(await repository.UpdateAsync(first));
        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.UpdateAsync(second));
        var stored = (await repository.GetByIdAsync(user.Id))!;
        Assert.Equal(first.SessionVersion, stored.SessionVersion);
        Assert.Equal("new-hash", stored.PasswordHash);
    }

    [Fact]
    public async Task LegacyDocumentWithoutVersion_CanBeUpdatedOnce()
    {
        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        var project = new Project("Legacy", "Description", Guid.NewGuid(), ProjectStatus.Active, Guid.NewGuid());
        await repository.AddAsync(project);
        await database.Projects.UpdateOneAsync(p => p.Id == project.Id,
            Builders<Project>.Update.Unset(p => p.Version));
        var first = (await repository.GetByIdAsync(project.Id))!;
        var stale = (await repository.GetByIdAsync(project.Id))!;
        first.Update("New name", null, null, null);
        Assert.True(await repository.UpdateAsync(first));
        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.UpdateAsync(stale));
    }

    [Fact]
    public async Task StaleBrowserVersion_ReturnsConflictWithoutChangingProject()
    {
        using var client = factory.CreateClient();
        var login = await new UserTestHelper(client).CreateUserAndLoginAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        var team = await new TeamTestHelper(client).CreateTeamAsync();
        var project = await new ProjectTestHelper(client).CreateProjectAsync(team.Id);
        var first = await client.PutAsJsonAsync($"/api/project/{project.Id}", new
            { Name = "First edit", Version = project.Version });
        first.EnsureSuccessStatusCode();
        var stale = await client.PutAsJsonAsync($"/api/project/{project.Id}", new
            { Name = "Stale edit", Version = project.Version });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        var stored = await client.GetFromJsonAsync<ProjectResponseDto>($"/api/project/{project.Id}");
        Assert.Equal("First edit", stored!.Name);
    }
}
