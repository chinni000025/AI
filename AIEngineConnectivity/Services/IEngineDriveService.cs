using AIEngineConnectivity.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IEngineDriveService
    {
        public Task<long> InitiateFileUpload(UploadInitiateRequest request, CancellationToken cancellationToken);
    }
}
