import { Injectable } from '@angular/core';
import { defer, map, Observable } from 'rxjs';
import { ChunkInitalize, ChunkResult, ChunkUpload, EngineControllers, InitiateUploadRequest } from './engine-route-constants';
import { form } from '@angular/forms/signals';
import { EngineCore } from './engine-core';
@Injectable({
  providedIn: 'root',
})
export class FileUploadService {
  constructor(private engineCore: EngineCore) { }
  initializeUpload(initiateUpload: InitiateUploadRequest): Observable<ChunkInitalize> {
    return this.engineCore.post(`${EngineControllers.EngineDriveController}/initiate-upload`, initiateUpload);
  }

  uploadChunk(Data: ChunkUpload): Observable<ChunkResult> {
    return defer(() => {
      const startTime = performance.now();
      const formData = new FormData();
      formData.append('chunk', Data.chunk);
      formData.append('chunkIndex', Data.index.toString());
      formData.append('sessionId', Data.sessionId);
      return this.engineCore.post(`${EngineControllers.EngineDriveController}/uploadChunks`, formData)
        .pipe(map((response) => {
          const endTime = performance.now();
          return {
            response,
            durationMs: endTime - startTime
          }
        }));
    })
  }

  finalize(sessionId: any) {
    return this.engineCore.post(`${EngineControllers.EngineDriveController}/finalize`, { sessionId });
  }
}
