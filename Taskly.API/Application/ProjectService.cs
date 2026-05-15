using Taskly.Application.Results;
using Taskly.Application.DTOs;
using Taskly.Domain.Entities;
using Taskly.Domain;

namespace Taskly.Application
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITeamService _teamService;
        private readonly IUserService _userService;

        public ProjectService(IProjectRepository projectRepository, ITeamService teamService, IUserService userService)
        {
            _projectRepository = projectRepository;
            _teamService = teamService;
            _userService = userService;
        }

        public async Task<StructuredOperationResult<Project>> AddProjectAsync(CreateProjectDto projectDto, Guid ownerId)
        {
            var team = await _teamService.GetByIdAsync(projectDto.TeamId);
            if (team == null)
                return StructuredOperationResult<Project>.Fail(ProjectErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult<Project>.Fail(ProjectErrors.TeamInactive);
            if (String.IsNullOrWhiteSpace(projectDto.Name))
                return StructuredOperationResult<Project>.Fail(ProjectErrors.InvalidName);

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

        public async Task<StructuredOperationResult<Project>> UpdateProjectAsync(Guid id, UpdateProjectDto updateProjectDto)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if(project == null)
                return StructuredOperationResult<Project>.Fail(ProjectErrors.NotFound);
            
            if(updateProjectDto.OwnerId!= null)
            {
                var user = await _userService.GetByIdAsync(updateProjectDto.OwnerId.Value);
                if(user == null)
                    return StructuredOperationResult<Project>.Fail(ProjectErrors.OwnerNotFound);
            }
            if(updateProjectDto.TeamId!= null)
            {
                var team = await _teamService.GetByIdAsync(updateProjectDto.TeamId.Value);
                if(team == null)
                    return StructuredOperationResult<Project>.Fail(ProjectErrors.TeamNotFound);
            }
            

            project.Update(updateProjectDto.Name, updateProjectDto.Description, updateProjectDto.OwnerId, updateProjectDto.Status, updateProjectDto.TeamId);

            var updated = await _projectRepository.UpdateAsync(project);
            if (!updated)
                return StructuredOperationResult<Project>.Fail(ProjectErrors.NotFound);

            return StructuredOperationResult<Project>.Ok(project);
        }

        public async Task<bool> DeleteProjectAsync(Guid id)
        {
           return await _projectRepository.DeleteAsync(id); 
        }
    }
}
