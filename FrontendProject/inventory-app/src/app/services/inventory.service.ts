import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
import { Inventory, CreateInventoryRequest, CreateInventoryProductRequest, InventoryManager, InventoryManagerAssignmentRequest, UpdateInventoryRequest, UpdateMinStockRequest, ManagerUser, InventoryProduct, QuantityChangeRequest, SetQuantityRequest, InventoriesForProduct, ProductsForInventories } from '../models/inventory';
import { Product } from '../models/product';
import { PaginationResponse } from '../models/pagination-response';
@Injectable({
  providedIn: 'root'
})

export class InventoryService {

  private baseUrl = environment.apiUrl; 

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  
  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.accessToken;
    if (!token) {
      throw new Error('Authentication required. No access token found.');
    }
    return new HttpHeaders({
      'accept': 'text/plain',
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json' // Most endpoints expect JSON body
    });
  }

  
  private handleError(error: any, context: string) {
    console.error(`Error in ${context}:`, error);
    let errorMessage = `Failed to ${context}. Please try again.`;

    if (error.status === 404) {
      errorMessage = `Resource not found for ${context}.`;
    } else if (error.status === 401 || error.status === 403) {
      errorMessage = 'Unauthorized: You do not have permission to perform this action.';
      this.authService.logout(); // Redirect to login or handle session expiry
    } else if (error.status === 400) {
      errorMessage = `Bad Request: ${error.error?.detail || error.message || 'Invalid data provided.'}`;
    } else if (error.status === 409) {
      errorMessage = `Conflict: ${error.error?.detail || error.message || 'Resource already exists or operation conflicts.'}`;
    } else if (error.status === 422) {
      errorMessage = `Unprocessable Content: ${error.error?.detail || error.message || 'The request was well-formed but could not be processed.'}`;
    } else if (error.status === 500) {
      errorMessage = 'Server error. Please try again later.';
    }
    return throwError(() => new Error(errorMessage));
  }

  //INVENTORY Endpoints


  getAllInventories(pageNumber: number | null = null, pageSize: number | null = null, searchTerm: string | null = null, orderBy: string | null = null, includeDeleted: boolean = false): Observable<PaginationResponse<Inventory>> {
    let params = new HttpParams();
    params = params.set('includeDeleted', includeDeleted.toString());
    if (pageNumber !== null) params = params.set('pageNumber', pageNumber);
    if (pageSize !== null) params = params.set('pageSize', pageSize);
    if (searchTerm !== null) params = params.set('searchTerm', searchTerm);
    if (orderBy !== null) params = params.set('orderBy', orderBy);

    return this.http.get<PaginationResponse<Inventory>>(`${this.baseUrl}/api/v1/Inventories`, { headers: this.getAuthHeaders(), params }).pipe(
      catchError(error => this.handleError(error, 'fetching all inventories'))
    );
  }

  
  getInventoryById(inventoryId: number): Observable<Inventory> {
    return this.http.get<Inventory>(`${this.baseUrl}/api/v1/Inventories/${inventoryId}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `fetching inventory with ID ${inventoryId}`))
    );
  }

  
  createInventory(inventoryData: CreateInventoryRequest): Observable<Inventory> {
    return this.http.post<Inventory>(`${this.baseUrl}/api/v1/Inventories`, inventoryData, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'creating inventory'))
    );
  }

  
  updateInventory(inventoryData: UpdateInventoryRequest): Observable<Inventory> {
    return this.http.put<Inventory>(`${this.baseUrl}/api/v1/Inventories`, inventoryData, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'updating inventory'))
    );
  }

  
  softDeleteInventory(inventoryId: number): Observable<Inventory> {
    return this.http.delete<Inventory>(`${this.baseUrl}/api/v1/Inventories/softdelete/${inventoryId}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `soft deleting inventory with ID ${inventoryId}`))
    );
  }

  
  hardDeleteInventory(inventoryId: number): Observable<Inventory> {
    return this.http.delete<Inventory>(`${this.baseUrl}/api/v1/Inventories/harddelete/${inventoryId}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `hard deleting inventory with ID ${inventoryId}`))
    );
  }

  // INVENTORY MANAGERS Endpoints

  
  getInventoriesByManagerId(managerId: number, sortBy?: string): Observable<Inventory[]> {
    let params = new HttpParams();
    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }

    return this.http.get<Inventory[]>(`${this.baseUrl}/api/v1/InventoryManagers/by-manager/${managerId}`, { headers: this.getAuthHeaders(), params }).pipe(
      catchError(error => this.handleError(error, `fetching inventories for manager ${managerId}`))
    );
  }

  
  getManagersByInventoryId(inventoryId: number, sortBy?: string): Observable<ManagerUser[]> {
    let params = new HttpParams();
    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }

    return this.http.get<ManagerUser[]>(`${this.baseUrl}/api/v1/InventoryManagers/by-inventory/${inventoryId}`, { headers: this.getAuthHeaders(), params }).pipe(
      catchError(error => this.handleError(error, `fetching managers for inventory ${inventoryId}`))
    );
  }

  
  getAllInventoryManagers(): Observable<InventoryManager[]> {
    return this.http.get<InventoryManager[]>(`${this.baseUrl}/api/v1/InventoryManagers`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'fetching all inventory managers'))
    );
  }

  
  assignInventoryToManager(assignmentData: InventoryManagerAssignmentRequest): Observable<InventoryManager> {
    return this.http.post<InventoryManager>(`${this.baseUrl}/api/v1/InventoryManagers/assign`, assignmentData, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'assigning inventory to manager'))
    );
  }

  
  removeInventoryFromManager(removalData: InventoryManagerAssignmentRequest): Observable<InventoryManager> {
    
    const options = {
      headers: this.getAuthHeaders(),
      body: removalData // Body for DELETE request
    };
    return this.http.delete<InventoryManager>(`${this.baseUrl}/api/v1/InventoryManagers/remove`, options).pipe(
      catchError(error => this.handleError(error, 'removing inventory from manager'))
    );
  }

  // --- INVENTORY PRODUCTS Endpoints ---

  
  getAllInventoryProducts(sortBy?: string): Observable<InventoryProduct[]> {
    let params = new HttpParams();
    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }
    return this.http.get<InventoryProduct[]>(`${this.baseUrl}/api/v1/InventoryProducts`, { headers: this.getAuthHeaders(), params }).pipe(
      catchError(error => this.handleError(error, 'fetching all inventory products'))
    );
  }

  
  getInventoryProductById(inventoryProductId: number): Observable<InventoryProduct> {

    return this.http.get<InventoryProduct>(`${this.baseUrl}/api/v1/InventoryProducts/${inventoryProductId}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `fetching inventory product with ID ${inventoryProductId}`))
    );
  }

  
  getInventoryProductByInventoryAndProductId(inventoryId: number, productId: number): Observable<InventoryProduct> {
    return this.http.get<InventoryProduct>(`${this.baseUrl}/api/v1/InventoryProducts/by-inventory/${inventoryId}/product/${productId}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `fetching product ${productId} in inventory ${inventoryId}`))
    );
  }

  
  getProductsInInventory(inventoryId: number, pageNumber: number | null = null, pageSize: number | null = null, searchTerm: string | null = null, orderBy: string | null = null): Observable<PaginationResponse<ProductsForInventories>> {

    let params = new HttpParams();
    if (pageNumber !== null) params = params.set('pageNumber', pageNumber);
    if (pageSize !== null) params = params.set('pageSize', pageSize);
    if (searchTerm !== null) params = params.set('searchTerm', searchTerm);
    if (orderBy !== null) params = params.set('orderBy', orderBy);

    return this.http.get<PaginationResponse<ProductsForInventories>>(`${this.baseUrl}/api/v1/InventoryProducts/products-in-inventory/${inventoryId}`, { headers: this.getAuthHeaders(), params }).pipe(
      catchError(error => this.handleError(error, `fetching products in inventory ${inventoryId}`))
    );
  }

  
  getProductsInInventoryByCategory(inventoryId: number, categoryId: number, sortBy?: string): Observable<Product[]> {
    let params = new HttpParams();
    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }
    return this.http.get<Product[]>(`${this.baseUrl}/api/v1/InventoryProducts/products-in-inventory/${inventoryId}/by-category/${categoryId}`, { headers: this.getAuthHeaders(), params }).pipe(
      catchError(error => this.handleError(error, `fetching products in inventory ${inventoryId} by category ${categoryId}`))
    );
  }

  
  getInventoriesForProduct(productId: number, pageNumber: number | null = null, pageSize: number | null = null, searchTerm: string | null = null, orderBy: string | null = null): Observable<PaginationResponse<InventoriesForProduct>> {
    let params = new HttpParams();
    if (pageNumber !== null) params = params.set('pageNumber', pageNumber);
    if (pageSize !== null) params = params.set('pageSize', pageSize);
    if (searchTerm !== null) params = params.set('searchTerm', searchTerm);
    if (orderBy !== null) params = params.set('orderBy', orderBy);

    console.log("prdId2:",productId);

    return this.http.get<PaginationResponse<InventoriesForProduct>>(`${this.baseUrl}/api/v1/InventoryProducts/inventories-for-product/${productId}`, { headers: this.getAuthHeaders(), params }).pipe(
      catchError(error => this.handleError(error, `fetching inventories for product ${productId}`))
    );
  }

  
  getInventoriesForProductBySku(sku: string, sortBy?: string): Observable<Inventory[]> {
    let params = new HttpParams();
    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }
    return this.http.get<Inventory[]>(`${this.baseUrl}/api/v1/InventoryProducts/inventories-for-product-by-sku/${sku}`, { headers: this.getAuthHeaders(), params }).pipe(
      catchError(error => this.handleError(error, `fetching inventories for product SKU ${sku}`))
    );
  }

  
  getLowStockProducts(inventoryId: number, threshold: number, sortBy?: string): Observable<Product[]> {
    let params = new HttpParams();
    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }
    return this.http.get<Product[]>(`${this.baseUrl}/api/v1/InventoryProducts/low-stock/${inventoryId}/${threshold}`, { headers: this.getAuthHeaders(), params }).pipe(
      catchError(error => this.handleError(error, `fetching low stock products in inventory ${inventoryId}`))
    );
  }

  
  createInventoryProduct(inventoryProductData: CreateInventoryProductRequest): Observable<InventoryProduct> {
    return this.http.post<InventoryProduct>(`${this.baseUrl}/api/v1/InventoryProducts`, inventoryProductData, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'creating inventory product'))
    );
  }

  
  increaseInventoryProductQuantity(data: QuantityChangeRequest): Observable<InventoryProduct> {
    return this.http.put<InventoryProduct>(`${this.baseUrl}/api/v1/InventoryProducts/increase-quantity`, data, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'increasing inventory product quantity'))
    );
  }

  
  decreaseInventoryProductQuantity(data: QuantityChangeRequest): Observable<InventoryProduct> {
    return this.http.put<InventoryProduct>(`${this.baseUrl}/api/v1/InventoryProducts/decrease-quantity`, data, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'decreasing inventory product quantity'))
    );
  }

  
  setInventoryProductQuantity(data: SetQuantityRequest): Observable<InventoryProduct> {
    return this.http.put<InventoryProduct>(`${this.baseUrl}/api/v1/InventoryProducts/set-quantity`, data, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'setting inventory product quantity'))
    );
  }

  
  updateInventoryProductMinStock(data: UpdateMinStockRequest): Observable<InventoryProduct> {
    return this.http.put<InventoryProduct>(`${this.baseUrl}/api/v1/InventoryProducts/update-minstock`, data, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, 'updating inventory product minimum stock quantity'))
    );
  }

  
  deleteInventoryProduct(inventoryProductId: number): Observable<InventoryProduct> {
    return this.http.delete<InventoryProduct>(`${this.baseUrl}/api/v1/InventoryProducts/${inventoryProductId}`, { headers: this.getAuthHeaders() }).pipe(
      catchError(error => this.handleError(error, `deleting inventory product with ID ${inventoryProductId}`))
    );
  }
}