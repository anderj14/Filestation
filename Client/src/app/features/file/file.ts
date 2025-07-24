import {Component, inject, OnInit, signal} from '@angular/core';
import {Fileservice} from '../../core/services/fileservice';
import {StorageInfo} from '../../shared/model/file';
import {DatePipe} from '@angular/common';
import {UploadFile} from './upload-file/upload-file';

@Component({
  selector: 'app-file',
  imports: [
    DatePipe,
    UploadFile
  ],
  standalone: true,
  templateUrl: './file.html',
  styleUrl: './file.scss'
})
export class File implements OnInit {
  private fileService = inject(Fileservice);
  storageInfo = this.fileService.storageInfo;

  ngOnInit() {
    this.fileService.getFiles();
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / 1024 / 1024).toFixed(2) + ' MB';
  }

  deleteFile(id: string) {
    if (!confirm('¿Seguro que quieres eliminar este archivo?')) return;

    this.fileService.deleteFile(id).subscribe({
      error: err => console.error('Error al eliminar archivo:', err)
    });
  }

  downloadFile(id: string, fileName: string) {
    this.fileService.downloadFile(id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Error descargando archivo:', err);
        alert('Error al descargar el archivo.');
      }
    })
  }
}
