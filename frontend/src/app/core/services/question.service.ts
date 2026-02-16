import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

export interface Question {
  id: string;
  title: string;
  slug: string;
  body?: string;
  status: string;
  viewCount: number;
  voteScore: number;
  answerCount: number;
  commentCount: number;
  bookmarkCount: number;
  author: QuestionAuthor;
  tags: Tag[];
  createdAt: string;
  updatedAt: string;
  lastActivityAt: string;
}

export interface Answer {
  id: string;
  body: string;
  isAccepted: boolean;
  voteScore: number;
  createdAt: string;
}

export interface QuestionDetail extends Question {
  answers: Answer[];
  /** API returns flat author fields when using GetQuestion endpoint */
  authorDisplayName?: string;
  authorUsername?: string;
}

export interface QuestionAuthor {
  id: string;
  username: string;
  displayName: string;
  reputation: number;
  avatarUrl?: string;
}

export interface Tag {
  id: string;
  name: string;
  description?: string;
  usageCount: number;
}

export interface CreateQuestionRequest {
  title: string;
  body: string;
  tags: string[];
  /** Optional: Share your solution with the question. Will be posted as an accepted answer. */
  solution?: string;
}

export interface CreateQuestionResponse {
  id: string;
  slug: string;
  title: string;
  createdAt: string;
}

export interface PaginatedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface QuestionFilters {
  page?: number;
  pageSize?: number;
  sortBy?: 'newest' | 'votes' | 'active' | 'unanswered';
  tags?: string[];
  status?: string;
  searchQuery?: string;
}

@Injectable({
  providedIn: 'root'
})
export class QuestionService {
  private apiUrl = `${environment.apiUrl}/questions`;

  constructor(private http: HttpClient) {}

  getQuestions(filters: QuestionFilters = {}): Observable<PaginatedResult<Question>> {
    let params = new HttpParams();
    
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.searchQuery) params = params.set('q', filters.searchQuery);
    if (filters.tags && filters.tags.length > 0) {
      filters.tags.forEach(tag => params = params.append('tags', tag));
    }

    return this.http.get<PaginatedResult<Question>>(this.apiUrl, { params });
  }

  getQuestionById(id: string): Observable<Question> {
    return this.http.get<Question>(`${this.apiUrl}/${id}`);
  }

  getQuestionBySlug(slug: string): Observable<QuestionDetail> {
    // Backend supports: GET /api/questions/{idOrSlug}
    return this.http.get<QuestionDetail>(`${this.apiUrl}/${slug}`);
  }

  createQuestion(request: CreateQuestionRequest): Observable<CreateQuestionResponse> {
    return this.http.post<CreateQuestionResponse>(this.apiUrl, request);
  }

  updateQuestion(id: string, request: Partial<CreateQuestionRequest>): Observable<Question> {
    return this.http.put<Question>(`${this.apiUrl}/${id}`, request);
  }

  deleteQuestion(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  voteQuestion(id: string, voteType: 'upvote' | 'downvote'): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/vote`, { voteType });
  }

  bookmarkQuestion(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/bookmark`, {});
  }

  getSimilarQuestions(id: string): Observable<Question[]> {
    return this.http.get<Question[]>(`${this.apiUrl}/${id}/similar`);
  }
}
