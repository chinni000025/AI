namespace AIEngineConnectivity.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IProjectRepository
    {
        public Task<List<ProjectDTO>> GetAllProjects(string userId, CancellationToken cancellationToken);
    }
}
