using Taskly.Application.Results;
using Taskly.Application.DTOs;
using Taskly.Domain.Entities;
using Taskly.Domain;

namespace Taskly.Application
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;

        public ProjectService(IProjectRepository projectRepository, ITeamRepository teamRepository, IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _teamRepository = teamRepository;
            _userRepository = userRepository;
        }

        public async Task<StructuredOperationResult<ProjectResponseDto>> AddProjectAsync(CreateProjectDto createProjectDto, Guid authenticatedUserId)
        {
            if (String.IsNullOrWhiteSpace(createProjectDto.Name))
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.InvalidName);

            var team = await _teamRepository.GetByIdAsync(createProjectDto.TeamId);

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

        

        public async Task<ProjectResponseDto?> GetByIdAsync(Guid id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if(project == null)
                return null;

            var projectResponseDto = new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                OwnerId = project.OwnerId,
                Status = project.Status,
                TeamId = project.TeamId
            };

            return projectResponseDto;
        }

        public async Task<StructuredOperationResult<ProjectResponseDto>> UpdateProjectAsync(Guid id, UpdateProjectDto updateProjectDto, Guid authenticatedUserId)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            var permission = await CanManageProject(project, authenticatedUserId);

            if (permission != null)
                return permission;

            if(updateProjectDto.TeamId!= null)
            {
                var newTeam = await _teamRepository.GetByIdAsync(updateProjectDto.TeamId.Value);
                if(newTeam == null)
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamNotFound);
                if (!newTeam.IsActive)
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamInactive);

                if(!newTeam.UserIds.Contains(authenticatedUserId))
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.NotAuthorized);
            }
            

            project!.Update(updateProjectDto.Name, updateProjectDto.Description, updateProjectDto.Status, updateProjectDto.TeamId);

            var updated = await _projectRepository.UpdateAsync(project);
            if (!updated)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.NotFound);

            var projectResponseDto = new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                OwnerId = project.OwnerId,
                Status = project.Status,
                TeamId = project.TeamId
            };

            return StructuredOperationResult<ProjectResponseDto>.Ok(projectResponseDto);
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

        public async Task<StructuredOperationResult<List<ProjectResponseDto>>> GetTeamProjectsAsync(Guid teamId, Guid authenticatedUserId)
        {
            var user = await _userRepository.GetByIdAsync(authenticatedUserId);
            if(user == null)
                return StructuredOperationResult<List<ProjectResponseDto>>.Fail(TeamErrors.UserNotFound);

            var team = await _teamRepository.GetByIdAsync(teamId);
            if(team == null)
                return StructuredOperationResult<List<ProjectResponseDto>>.Fail(ProjectErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult<List<ProjectResponseDto>>.Fail(ProjectErrors.TeamInactive);
            if(!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<List<ProjectResponseDto>>.Fail(ProjectErrors.UserNotTeamMember);
            
            var projects = await _projectRepository.GetByTeamIdAsync(teamId);

              var response = projects
                .Select(project => new ProjectResponseDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    OwnerId = project.OwnerId,
                    Status = project.Status,
                    TeamId = project.TeamId
                })
                .ToList();
            
            return StructuredOperationResult<List<ProjectResponseDto>>.Ok(response);
        }

        private async Task<StructuredOperationResult<ProjectResponseDto>?> CanManageProject(Project? project, Guid authenticatedUserId)
        {
            if(project == null)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.NotFound);

            if (project.OwnerId == authenticatedUserId)
                return null;
                
            var team = await _teamRepository.GetByIdAsync(project.TeamId);

            if (team == null)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamNotFound);


            if(team.OwnerId != authenticatedUserId)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.NotAuthorized);

            return null;
        }


    }
}
