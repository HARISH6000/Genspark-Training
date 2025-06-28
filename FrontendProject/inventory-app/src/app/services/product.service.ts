// src/app/services/product.service.ts

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
import { Product, AddProductRequest, UpdateProductRequest } from '../models/product';
import { PaginationResponse } from '../models/pagination-response';



@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private baseUrl = environment.apiUrl;
  private productsApiUrl = `${this.baseUrl}/api/v1/Products`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }


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

  private handleError(error: any, context: string) {
    console.error(`Error in ${context}:`, error);
    let errorMessage = `Failed to ${context}. Please try again.`;

    if (error.status === 404) {
      errorMessage = `Resource not found for ${context}.`;
    } else if (error.status === 401 || error.status === 403) {
      errorMessage = 'Unauthorized: You do not have permission to perform this action.';
      this.authService.logout();
    } else if (error.status === 400) {
      errorMessage = `Bad Request: ${error.error?.message || error.error || 'Invalid data provided.'}`;
    } else if (error.status === 409) {
      errorMessage = `Conflict: ${error.error?.message || error.error || 'Resource already exists or operation conflicts.'}`;
    } else if (error.status === 500) {
      errorMessage = 'Server error. Please try again later.';
    }

    if (error.error && error.error.message) {
      errorMessage = error.error.message;
    }
    return throwError(() => new Error(errorMessage));
  }


  addProduct(productData: AddProductRequest): Observable<Product> {
    return this.http.post<Product>(this.productsApiUrl, productData, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'adding product'))
    );
  }

  getProductById(productId: number): Observable<Product> {
    return this.http.get<Product>(`${this.productsApiUrl}/${productId}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `fetching product with ID ${productId}`))
    );
  }

  getAllProducts(pageNumber: number | null = null, pageSize: number | null = null, searchTerm: string | null = null, orderBy: string | null = null, includeDeleted: boolean = false): Observable<PaginationResponse<Product>> {
    let params = new HttpParams();
    params = params.set('includeDeleted', includeDeleted.toString());
    if (pageNumber !== null) params = params.set('pageNumber', pageNumber);
    if (pageSize !== null) params = params.set('pageSize', pageSize);
    if (searchTerm !== null) params = params.set('searchTerm', searchTerm);
    if (orderBy !== null) params = params.set('orderBy', orderBy);

    return this.http.get<PaginationResponse<Product>>(this.productsApiUrl, { headers: this.getAuthHeaders(), params }).pipe(
      catchError(error => this.handleError(error, 'fetching all products'))
    );
  }

  updateProduct(productData: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(this.productsApiUrl, productData, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'updating product'))
    );
  }

  softDeleteProduct(productId: number): Observable<Product> {
    return this.http.delete<Product>(`${this.productsApiUrl}/softdelete/${productId}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `soft deleting product with ID ${productId}`))
    );
  }

  hardDeleteProduct(productId: number): Observable<Product> {
    return this.http.delete<Product>(`${this.productsApiUrl}/harddelete/${productId}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `hard deleting product with ID ${productId}`))
    );
  }
}
