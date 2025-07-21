import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Video } from '../models/video.model'
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class VideoService {
  private apiUrl = 'http://localhost:5157/api/videos';

  constructor(private http: HttpClient) {}

  getVideos(): Observable<Video[]> {
    return this.http.get<Video[]>(this.apiUrl);
  }

  uploadVideo(formData: FormData): Observable<Video> {
    return this.http.post<Video>(`${this.apiUrl}/upload`, formData);
  }
}
