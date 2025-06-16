import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root' 
})
export class ProductService {
  private apiUrl = 'https://dummyjson.com/products/search'; 

  constructor(private http: HttpClient) { }

 
  getProducts(searchTerm: string, limit: number, skip: number): Observable<any> {
    let params = new HttpParams()
      .set('limit', limit.toString()) 
      .set('skip', skip.toString()); 

    if (searchTerm) {
      params = params.set('q', searchTerm); 
    }
    
    return this.http.get(this.apiUrl, { params });
  }
}
