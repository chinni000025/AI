import { Injectable } from '@angular/core';
import { defer, map, Observable } from 'rxjs';
import { ChunkInitalize, ChunkResult, ChunkUpload, EngineConstants, EngineControllers, InitiateUploadRequest } from './engine-route-constants';
import { form } from '@angular/forms/signals';
import { EngineCore } from './engine-core';
@Injectable({
  providedIn: 'root',
})
export class FileUploadService {
  constructor(private engineCore: EngineCore) { }
  private readonly _uploadSessionId = EngineConstants.UploadingSessionId;
  initializeUpload(initiateUpload: InitiateUploadRequest): Observable<ChunkInitalize> {
    return this.engineCore.post(`${EngineControllers.EngineDriveController}/initiate-upload`, initiateUpload);
  }

  uploadChunk(Data: ChunkUpload): Observable<ChunkResult> {
    return defer(() => {
      const startTime = performance.now();
      const formData = new FormData();
      formData.append('chunk', Data.chunk);
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

  private getUploadSessions(): Record<string, string> {
    var raw = localStorage.getItem(this._uploadSessionId);
    return raw ? JSON.parse(raw) : {};
  }

  private saveUploadSessions(map: Record<string, string>) {
    localStorage.setItem(this._uploadSessionId, JSON.stringify(map));
  }

  setUploadSessionId(fileKey: string, sessionId: string) {
    var map = this.getUploadSessions();
    map[fileKey] = sessionId;
    this.saveUploadSessions(map);
  }

  getUploadSessionId(fileKey: string): string | null {
    return this.getUploadSessions()[fileKey] ?? null;
  }

  removeUploadSessionId(fileKey: string) {
    var map = this.getUploadSessions();
    delete map[fileKey];
    this.saveUploadSessions(map);
  }
}
