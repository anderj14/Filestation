import {inject, Injectable, signal} from '@angular/core';
import {environment} from '../../../environments/environment.development';
import {HttpClient, HttpEvent, HttpEventType} from '@angular/common/http';
import {StorageInfo} from '../../shared/model/file';
import {Observable, tap} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Fileservice {
  private baseUrl = environment.apiUrl;
  http = inject(HttpClient);

  storageInfo = signal<StorageInfo | null>(null);

  getFiles() {
    this.http.get<StorageInfo>(`${this.baseUrl}files`).subscribe({
      next: (info) => this.storageInfo.set(info),
      error: (err) => {
        console.error('Error cargando archivos', err);
        this.storageInfo.set(null);
      }
    });
  }

  uploadFile(file: File): Observable<HttpEvent<any>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}upload`, formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      tap(event => {
        if (event.type === HttpEventType.Response) {
          this.getFiles();
        }
      })
    );
  }

  deleteFile(id: string) {
    return this.http.delete(`${this.baseUrl}files/${id}`).pipe(
      tap(() => this.getFiles())
    );
  }

  downloadFile(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}download/${id}`, { responseType: 'blob' });
  }

}
