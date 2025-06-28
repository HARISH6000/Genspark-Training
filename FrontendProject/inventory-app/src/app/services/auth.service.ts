import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  userId: number;
  username: string;
  token: string;
  refreshToken: string;
}

export interface UserDetails {
  userId: number;
  username: string;
  email: string;
  phone: string;
  profilePictureUrl:string,
  roleId: number;
  roleName: string;
  isDeleted: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5085/api/v1'; 

  private currentUserSubject: BehaviorSubject<UserDetails | null>;
  public currentUser: Observable<UserDetails | null>;

  constructor(
    private http: HttpClient,
    private router: Router
  ) {

    const storedUser = localStorage.getItem('currentUser');
    this.currentUserSubject = new BehaviorSubject<UserDetails | null>(storedUser ? JSON.parse(storedUser) : null);
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): UserDetails | null {
    return this.currentUserSubject.value;
  }

  public get accessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  public get refreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/Auth/login`, credentials).pipe(
      tap(response => {
        localStorage.setItem('accessToken', response.token);
        localStorage.setItem('refreshToken', response.refreshToken);
        localStorage.setItem('username', response.username);
      }),
      catchError(error => {
        console.error('Login failed:', error);
        return throwError(() => new Error('Login failed. Please check your credentials.'));
      })
    );
  }

  getUserDetails(username: string): Observable<UserDetails> {
    const token = this.accessToken;
    if (!token) {
      this.logout();
      return throwError(() => new Error('No access token found.'));
    }

    const headers = {
      'accept': 'text/plain',
      'Authorization': `Bearer ${token}`
    };

    return this.http.get<UserDetails>(`${this.apiUrl}/Users/by-username/${username}`, { headers }).pipe(
      tap(userDetails => {
        localStorage.setItem('currentUser', JSON.stringify(userDetails));
        this.currentUserSubject.next(userDetails);
      }),
      catchError(error => {
        console.error('Failed to fetch user details:', error);
        if (error.status === 401 || error.status === 403) {
          this.logout();
        }
        return throwError(() => new Error('Failed to fetch user details.'));
      })
    );
  }

  logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('username');
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  isAdmin(): boolean {
    return this.currentUserValue?.roleName?.toLowerCase() === 'admin';
  }

  isManager(): boolean {
    return this.currentUserValue?.roleName?.toLowerCase() === 'manager';
  }
}