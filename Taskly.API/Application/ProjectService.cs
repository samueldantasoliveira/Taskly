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

        public async Task<bool> AddProjectAsync(Project project)
        {
            var team = await _teamRepository.GetByIdAsync(project.TeamId);
            if (team == null || !team.IsActive)
                return false;

            if (String.IsNullOrEmpty(project.Name))
                throw new ArgumentException("Name must not be empty");
            await _projectRepository.AddAsync(project);
            return true;
        }
    }
}
