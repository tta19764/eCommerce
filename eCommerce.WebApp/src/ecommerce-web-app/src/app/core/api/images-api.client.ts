import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import { apiData } from './api-base';

export interface ImageResource {
  id: string;
  fileName: string;
  contentType: string;
  size: number;
  url: string;
  status: string;
  createdAtUtc: string;
}
@Injectable({ providedIn: 'root' })
export class ImagesApiClient {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.gatewayUrl}/image-api/v1/images`;
  contentUrl(id: string) {
    return `${this.url}/${id}/content`;
  }
  upload(file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ApiResponse<ImageResource>>(this.url, form).pipe(map(apiData));
  }
}
