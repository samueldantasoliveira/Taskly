using Taskly.Application.Results;
using Taskly.Application.DTOs;
using Taskly.Domain.Entities;
using Taskly.Domain;

namespace Taskly.Application
{
    public class ProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITeamRepository _teamRepository;

        public ProjectService(IProjectRepository projectRepository, ITeamRepository teamRepository)
        {
            _projectRepository = projectRepository;
            _teamRepository = teamRepository;
        }

        public async Task<StructuredOperationResult<Project>> AddProjectAsync(CreateProjectDto projectDto)
        {
            var team = await _teamRepository.GetByIdAsync(projectDto.TeamId);
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
                ProjectStatus.Active
            );
            
            await _projectRepository.AddAsync(project);
            return StructuredOperationResult<Project>.Ok(project);
        }
    }
}
