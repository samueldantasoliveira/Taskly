using Taskly.Application.Results;
using Taskly.Domain.Entities;

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

        public async Task<OperationResult<AddProjectFailureReason>> AddProjectAsync(Project project)
        {
            var team = await _teamRepository.GetByIdAsync(project.TeamId);
            if (team == null)
                return OperationResult<AddProjectFailureReason>.Fail(AddProjectFailureReason.TeamNotFound);
            if (!team.IsActive)
                    return OperationResult<AddProjectFailureReason>.Fail(AddProjectFailureReason.TeamInactive);

            if (String.IsNullOrWhiteSpace(project.Name))
                return OperationResult<AddProjectFailureReason>.Fail(AddProjectFailureReason.InvalidName);

            await _projectRepository.AddAsync(project);
            return OperationResult<AddProjectFailureReason>.Ok();
        }
    }
}
