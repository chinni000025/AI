using AIEngineConnectivity.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Repositories
{
    public interface IProjectRepository
    {
        public Task<List<ProjectDTO>> GetAllProjects(string userId, CancellationToken cancellationToken);
    }
}
