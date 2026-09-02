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

        public async Task<StructuredOperationResult<ProjectResponseDto>> AddProjectAsync(CreateProjectDto createProjectDto, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrWhiteSpace(createProjectDto.Name))
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.InvalidName);

            var team = await _teamRepository.GetByIdAsync(createProjectDto.TeamId, cancellationToken);

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
            
            await _projectRepository.AddAsync(project, cancellationToken);
            return StructuredOperationResult<ProjectResponseDto>.Ok(projectResponseDto);
        }

        

        public async Task<StructuredOperationResult<ProjectResponseDto>> GetByIdAsync(Guid id, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var project = await _projectRepository.GetByIdAsync(id, cancellationToken);

            if(project == null)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.NotFound);
            
            var team = await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
            if(team == null)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamNotFound);

            if(!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.UserNotTeamMember);

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

        public async Task<StructuredOperationResult<ProjectResponseDto>> UpdateProjectAsync(Guid id, UpdateProjectDto updateProjectDto, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
            var permission = await CanManageProject(project, authenticatedUserId, cancellationToken);

            if (permission != null)
                return permission;

            if(updateProjectDto.TeamId!= null)
            {
                var newTeam = await _teamRepository.GetByIdAsync(updateProjectDto.TeamId.Value, cancellationToken);
                if(newTeam == null)
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamNotFound);
                if (!newTeam.IsActive)
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamInactive);

                if(!newTeam.UserIds.Contains(authenticatedUserId))
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.NotAuthorized);
            }
            

            project!.Update(updateProjectDto.Name, updateProjectDto.Description, updateProjectDto.Status, updateProjectDto.TeamId);

            var updated = await _projectRepository.UpdateAsync(project, cancellationToken);
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

        public async Task<StructuredOperationResult> DeleteProjectAsync(Guid id, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
            var permission = await CanManageProject(project, authenticatedUserId, cancellationToken);

            if(permission != null)
                return permission;
            
            var deleted = await _projectRepository.DeleteAsync(id, cancellationToken);

            if(!deleted)
                return StructuredOperationResult.Fail(ProjectErrors.NotFound);
            return StructuredOperationResult.Ok();
        }

        public async Task<StructuredOperationResult<List<ProjectResponseDto>>> GetTeamProjectsAsync(Guid teamId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(authenticatedUserId, cancellationToken);
            if(user == null)
                return StructuredOperationResult<List<ProjectResponseDto>>.Fail(TeamErrors.UserNotFound);

            var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
            if(team == null)
                return StructuredOperationResult<List<ProjectResponseDto>>.Fail(ProjectErrors.TeamNotFound);
            if (!team.IsActive)
                return StructuredOperationResult<List<ProjectResponseDto>>.Fail(ProjectErrors.TeamInactive);
            if(!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<List<ProjectResponseDto>>.Fail(ProjectErrors.UserNotTeamMember);
            
            var projects = await _projectRepository.GetByTeamIdAsync(teamId, cancellationToken);

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

        private async Task<StructuredOperationResult<ProjectResponseDto>?> CanManageProject(Project? project, Guid authenticatedUserId, CancellationToken cancellationToken)
        {
            if(project == null)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.NotFound);

            if (project.OwnerId == authenticatedUserId)
                return null;
                
            var team = await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);

            if (team == null)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamNotFound);


            if(team.OwnerId != authenticatedUserId)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.NotAuthorized);

            return null;
        }


    }
}
