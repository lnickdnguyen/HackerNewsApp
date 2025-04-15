import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Story {
  id: number;
  title: string;
  url: string;
}

@Injectable({ providedIn: 'root' })
export class StoryService {
  private apiUrl = 'https://localhost:5001/api/hackernews/getneweststories';

  constructor(private http: HttpClient) {}

  getStories(page: number, pageSize: number, search: string = ''): Observable<Story[]> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<Story[]>(this.apiUrl, { params });
  }
}
