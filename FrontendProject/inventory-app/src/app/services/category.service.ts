// src/app/services/category.service.ts

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';



export interface Category {
  categoryId: number;
  categoryName: string;
  description: string;
  productCount?: number; 
}


export interface CreateCategoryRequest {
  categoryName: string;
  description: string;
}


export interface UpdateCategoryRequest {
  categoryId: number;
  categoryName: string;
  description: string;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private baseUrl = environment.apiUrl;
  private categoriesApiUrl = `${this.baseUrl}/api/v1/Categories`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  /**
   * Helper method to get standard HttpHeaders with Authorization token.
   * Throws an error if no token is available, ensuring requests are authenticated.
   */
  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.accessToken;
    if (!token) {
      throw new Error('Authentication required. No access token found.');
    }
    return new HttpHeaders({
      'accept': 'text/plain',
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  /**
   * Helper method for common error handling logic.
   */
  private handleError(error: any, context: string) {
    console.error(`Error in ${context}:`, error);
    let errorMessage = `Failed to ${context}. Please try again.`;

    if (error.status === 404) {
      errorMessage = `Resource not found for ${context}.`;
    } else if (error.status === 401 || error.status === 403) {
      errorMessage = 'Unauthorized: You do not have permission to perform this action.';
      this.authService.logout(); // Redirect to login or handle session expiry
    } else if (error.status === 400) {
      errorMessage = `Bad Request: ${error.error?.message || error.message || 'Invalid data provided.'}`;
    } else if (error.status === 409) {
      errorMessage = `Conflict: ${error.error?.message || error.message || 'Resource already exists or operation conflicts.'}`;
    } else if (error.status === 500) {
      errorMessage = 'Server error. Please try again later.';
    }
    return throwError(() => new Error(errorMessage));
  }

  
  getAllCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(this.categoriesApiUrl, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'fetching all categories'))
    );
  }

  
  getCategoryById(categoryId: number): Observable<Category> {
    return this.http.get<Category>(`${this.categoriesApiUrl}/${categoryId}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `fetching category with ID ${categoryId}`))
    );
  }

  
  getCategoryByName(categoryName: string): Observable<Category> {
    return this.http.get<Category>(`${this.categoriesApiUrl}/by-name/${categoryName}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `fetching category by name "${categoryName}"`))
    );
  }

  
  createCategory(categoryData: CreateCategoryRequest): Observable<Category> {
    return this.http.post<Category>(this.categoriesApiUrl, categoryData, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'creating category'))
    );
  }

  
  updateCategory(categoryData: UpdateCategoryRequest): Observable<Category> {
    return this.http.put<Category>(this.categoriesApiUrl, categoryData, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'updating category'))
    );
  }

  
  deleteCategory(categoryId: number): Observable<Category> {
    return this.http.delete<Category>(`${this.categoriesApiUrl}/${categoryId}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `deleting category with ID ${categoryId}`))
    );
  }
}
