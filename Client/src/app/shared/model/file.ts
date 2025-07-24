export interface FileInfo {
  id: string;
  fileName: string;
  uploadDate: string;
  size: number;
}

export interface StorageInfo {
  storageUsedBytes: number;
  storageUsedMB: number;
  storageLimitBytes: number;
  storageLimitMB: number;
  files: FileInfo[];
}
