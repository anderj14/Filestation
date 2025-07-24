import {Component, inject, signal} from '@angular/core';
import {Fileservice} from '../../../core/services/fileservice';
import {HttpEvent, HttpEventType} from '@angular/common/http';

@Component({
  selector: 'app-upload-file',
  imports: [],
  templateUrl: './upload-file.html',
  styleUrl: './upload-file.scss'
})
export class UploadFile {
  fileService = inject(Fileservice);

  selectedFile = signal<File | null>(null);
  progress = signal<number>(0);
  message = signal<string | null>(null);
  error = signal<string | null>(null);

  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement)?.files?.[0];
    this.selectedFile.set(file || null);
    this.progress.set(0);
    this.message.set(null);
    this.error.set(null);
  }

  onUpload() {
    const file = this.selectedFile();
    if (!file) {
      this.error.set("You must select a file");
      return;
    }

    this.fileService.uploadFile(file).subscribe({
      next: (event: HttpEvent<any>) => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
          const percent = Math.round((100 * event.loaded) / event.total);
          this.progress.set(percent);
        } else if (event.type === HttpEventType.Response) {
          this.message.set('Archivo subido correctamente');
          this.selectedFile.set(null);
        }
      },
      error: err => {
        this.error.set(err.error || 'Error al subir el archivo');
      }
    });
  }
}
