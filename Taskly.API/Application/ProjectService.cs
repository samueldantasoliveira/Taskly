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
        private readonly ITodoTaskRepository _todoTaskRepository;

        public ProjectService(IProjectRepository projectRepository, ITeamRepository teamRepository, IUserRepository userRepository, ITodoTaskRepository todoTaskRepository)
        {
            _projectRepository = projectRepository;
            _teamRepository = teamRepository;
            _userRepository = userRepository;
            _todoTaskRepository = todoTaskRepository;
        }

        public async Task<StructuredOperationResult<ProjectResponseDto>> AddProjectAsync(CreateProjectDto createProjectDto, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            if (!IsValidName(createProjectDto.Name))
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.InvalidName);
            if (!IsValidDescription(createProjectDto.Description))
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.InvalidDescription);

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
                Version = project.Version,
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
                Version = project.Version,
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

            ConcurrencyConflictException.Check(updateProjectDto.Version, project!.Version);
            if (updateProjectDto.Name != null && !IsValidName(updateProjectDto.Name))
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.InvalidName);
            if (updateProjectDto.Description != null && !IsValidDescription(updateProjectDto.Description))
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.InvalidDescription);
            if (updateProjectDto.Status.HasValue && !Enum.IsDefined(updateProjectDto.Status.Value))
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.InvalidStatus);

            Team? destinationTeam = null;
            if (updateProjectDto.TeamId.HasValue && updateProjectDto.TeamId.Value != project.TeamId)
            {
                destinationTeam = await _teamRepository.GetByIdAsync(updateProjectDto.TeamId.Value, cancellationToken);
                if (destinationTeam == null)
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamNotFound);
                if (!destinationTeam.IsActive)
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamInactive);
                if (!destinationTeam.UserIds.Contains(authenticatedUserId))
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.NotAuthorized);
                var nextOwner = updateProjectDto.OwnerId ?? project.OwnerId;
                if (!destinationTeam.UserIds.Contains(nextOwner))
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.OwnerNotInDestinationTeam);
                var assignees = await _todoTaskRepository.GetAssignedUserIdsByProjectIdAsync(project.Id, cancellationToken);
                if (assignees.Any(id => !destinationTeam.UserIds.Contains(id)))
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.AssigneesNotInDestinationTeam);
            }

            if (updateProjectDto.OwnerId is Guid ownerId)
            {
                var ownerTeam = destinationTeam ?? await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
                if (ownerTeam == null || !ownerTeam.UserIds.Contains(ownerId))
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.UserNotTeamMember);
                if (await _userRepository.GetByIdAsync(ownerId, cancellationToken) == null)
                    return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.OwnerNotFound);
                project.TransferOwnership(ownerId);
            }
            

            project!.Update(updateProjectDto.Name, updateProjectDto.Description, updateProjectDto.Status, updateProjectDto.TeamId);

            var updated = await _projectRepository.UpdateAsync(project, cancellationToken);
            if (!updated)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.NotFound);

            var projectResponseDto = new ProjectResponseDto
            {
                Id = project.Id,
                Version = project.Version,
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
                    Version = project.Version,
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

            var team = await _teamRepository.GetByIdAsync(project.TeamId, cancellationToken);
            if (team == null)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamNotFound);

            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.UserNotTeamMember);

            if (!team.IsActive)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.TeamInactive);

            if (project.OwnerId != authenticatedUserId && team.OwnerId != authenticatedUserId)
                return StructuredOperationResult<ProjectResponseDto>.Fail(ProjectErrors.NotAuthorized);

            return null;
        }

        private static bool IsValidName(string? value) => !string.IsNullOrWhiteSpace(value) && value.Trim().Length is >= 2 and <= 100;
        private static bool IsValidDescription(string? value) => !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= 500;


    }
}
