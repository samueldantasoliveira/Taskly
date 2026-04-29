using Taskly.Application.Results;
using Taskly.Application.DTOs;
using Taskly.Domain.Entities;
using Taskly.Domain;
using Taskly.Infrastructure;

namespace Taskly.Application
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITeamService _teamService;

        public ProjectService(IProjectRepository projectRepository, ITeamService teamService)
        {
            _projectRepository = projectRepository;
            _teamService = teamService;
        }

        public async Task<StructuredOperationResult<Project>> AddProjectAsync(CreateProjectDto projectDto, Guid ownerId)
        {
            var team = await _teamService.GetByIdAsync(projectDto.TeamId);
            if (team == null)
                return StructuredOperationResult<Project>.Fail(Error.FromEnum(AddProjectFailureReason.TeamNotFound));
            if (!team.IsActive)
                return StructuredOperationResult<Project>.Fail(Error.FromEnum(AddProjectFailureReason.TeamInactive));
            if (String.IsNullOrWhiteSpace(projectDto.Name))
                return StructuredOperationResult<Project>.Fail(Error.FromEnum(AddProjectFailureReason.InvalidName));

            var project = new Project
            (
                projectDto.Name,
                projectDto.Description,
                projectDto.TeamId,
                ProjectStatus.Active,
                ownerId
            );
            
            await _projectRepository.AddAsync(project);
            return StructuredOperationResult<Project>.Ok(project);
        }

        public async Task<Project?> GetByIdAsync(Guid id)
        {
            return await _projectRepository.GetByIdAsync(id);
        }
    }
}
