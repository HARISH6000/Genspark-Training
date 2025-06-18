import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  gender: string;
  age: number;
  role: string; 
  image: string;
  address: {
    address: string;
    city: string;
    state: string;
    stateCode: string;
    postalCode: string;
    coordinates: {
      lat: number;
      lng: number;
    };
    country: string;
  };
}

export interface UserResponse {
  users: User[];
  total: number;
  skip: number;
  limit: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly baseUrl = 'https://dummyjson.com/users';

  constructor(private http: HttpClient) { }

  getAllUsers(): Observable<UserResponse> {
    return this.http.get<UserResponse>(this.baseUrl);
  }

  addUser(user: Partial<User>): Observable<User> {
    return this.http.post<User>(`${this.baseUrl}/add`, user, {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}