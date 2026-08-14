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

        public async Task<StructuredOperationResult<ProjectResponseDto>> AddProjectAsync(CreateProjectDto createProjectDto, Guid authenticatedUserId)
        {
            if (String.IsNullOrWhiteSpace(createProjectDto.Name))
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.InvalidName);

            var team = await _teamService.GetByIdAsync(createProjectDto.TeamId);

            if (team == null)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamInactive);
            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.UserNotTeamMember);
            var project = new Project
            (
                createProjectDto.Name,
                createProjectDto.Description,
                createProjectDto.TeamId,
                ProjectStatus.Active,
                authenticatedUserId
            );

            var projectResponseDto = new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                OwnerId = project.OwnerId,
                Status = project.Status,
                TeamId = project.TeamId
            };
            
            await _projectRepository.AddAsync(project);
            return StructuredOperationResult<ProjectResponseDto>.Ok(projectResponseDto);
        }

        public async Task<Project?> GetByIdAsync(Guid id)
        {
            return await _projectRepository.GetByIdAsync(id);
        }

        public async Task<StructuredOperationResult<Project>> UpdateProjectAsync(Guid id, UpdateProjectDto updateProjectDto, Guid authenticatedUserId)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            var permission = await CanManageProject(project, authenticatedUserId);

            if (permission != null)
                return permission;

            if(updateProjectDto.TeamId!= null)
            {
                var newTeam = await _teamService.GetByIdAsync(updateProjectDto.TeamId.Value);
                if(newTeam == null)
                    return StructuredOperationResult<Project>.Fail(ProjectErrors.TeamNotFound);
                if (!newTeam.IsActive)
                    return StructuredOperationResult<Project>.Fail(ProjectErrors.TeamInactive);

                if(!newTeam.UserIds.Contains(authenticatedUserId))
                    return StructuredOperationResult<Project>.Fail(ProjectErrors.NotAuthorized);
            }
            

            project!.Update(updateProjectDto.Name, updateProjectDto.Description, updateProjectDto.Status, updateProjectDto.TeamId);

            var updated = await _projectRepository.UpdateAsync(project);
            if (!updated)
                return StructuredOperationResult<Project>.Fail(ProjectErrors.NotFound);

            return StructuredOperationResult<Project>.Ok(project);
        }

        public async Task<StructuredOperationResult> DeleteProjectAsync(Guid id, Guid authenticatedUserId)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            var permission = await CanManageProject(project, authenticatedUserId);

            if(permission != null)
                return permission;
            
            var deleted = await _projectRepository.DeleteAsync(id);

            if(!deleted)
                return StructuredOperationResult.Fail(ProjectErrors.NotFound);
            return StructuredOperationResult.Ok();
        }

        private async Task<StructuredOperationResult<Project>?> CanManageProject(Project? project, Guid authenticatedUserId)
        {
            if(project == null)
                return StructuredOperationResult<Project>.Fail(ProjectErrors.NotFound);

            if (project.OwnerId == authenticatedUserId)
                return null;
                
            var team = await _teamService.GetByIdAsync(project.TeamId);

            if (team == null)
                return StructuredOperationResult<Project>.Fail(ProjectErrors.TeamNotFound);


            if(team.OwnerId != authenticatedUserId)
                return StructuredOperationResult<Project>.Fail(ProjectErrors.NotAuthorized);

            return null;
        }
    }
}
