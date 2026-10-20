using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Repositories;
using AIEngineGateway.EngineInfrastructure;

namespace AIEngineGateway.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly EngineContext _EngineContext;
        public ProjectRepository(EngineContext engineContext)
        {
            _EngineContext = engineContext;
        }

        public async Task<List<ProjectDTO>> GetAllProjects(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}