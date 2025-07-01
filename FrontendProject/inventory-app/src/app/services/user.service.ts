import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService, UserDetails } from './auth.service';
import { environment } from '../../environments/environment';
import { User, RegisterUserRequest, UpdateUserRequest, UpdateContactDetailsRequest, ChangePasswordRequest, Role } from '../models/user';
import { PaginationResponse } from '../models/pagination-response';
import { map } from 'rxjs/operators'; 


@Injectable({
    providedIn: 'root'
})
export class UserService {
    private baseUrl = environment.apiUrl
    private usersApiUrl = `${this.baseUrl}/api/v1/Users`;
    private rolesApiUrl = `${this.baseUrl}/api/v1/roles`;


    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }


    private getAuthHeaders(isFormData: boolean = false): HttpHeaders {
        const token = this.authService.accessToken;
        if (!token) {
            throw new Error('Authentication required. No access token found.');
        }
        let headers = new HttpHeaders({
            'accept': 'text/plain',
            'Authorization': `Bearer ${token}`
        });

        if (!isFormData) {
            headers = headers.set('Content-Type', 'application/json');
        }
        return headers;
    }


    private handleError(error: any, context: string) {
        console.error(`Error in ${context}:`, error);
        let errorMessage = `Failed to ${context}. Please try again.`;

        if (error.status === 404) {
            errorMessage = `Resource not found for ${context}.`;
        } else if (error.status === 401 || error.status === 403) {
            errorMessage = 'Unauthorized: You do not have permission to perform this action.';
            if(context !== 'fetching all users'){
                this.authService.logout();
            }
        } else if (error.status === 400) {
            errorMessage = `Bad Request: ${error.error?.message || error.error || 'Invalid data provided.'}`;
        } else if (error.status === 409) {
            errorMessage = `Conflict: ${error.error?.message || error.error || 'Resource already exists or operation conflicts.'}`;
        } else if (error.status === 422) {
            errorMessage = `Unprocessable Content: ${error.error?.detail || error.message || 'The request was well-formed but could not be processed.'}`;
        } else if (error.status === 500) {
            errorMessage = 'Server error. Please try again later.';
        }
        return throwError(() => new Error(errorMessage));
    }



    registerUser(user: RegisterUserRequest): Observable<UserDetails> {
        return this.http.post<UserDetails>(`${this.usersApiUrl}/register`, user, { headers: this.getAuthHeaders() }).pipe(
            catchError(error => this.handleError(error, 'user registration'))
        );
    }

    getAllUsers(pageNumber: number | null = null, pageSize: number | null = null, searchTerm: string | null = null, orderBy: string | null = null): Observable<PaginationResponse<User>> {

        let params = new HttpParams();

        if (pageNumber !== null) params = params.set('pageNumber', pageNumber);
        if (pageSize !== null) params = params.set('pageSize', pageSize);
        if (searchTerm !== null) params = params.set('searchTerm', searchTerm);
        if (orderBy !== null) params = params.set('orderBy', orderBy);

        console.log("inServ:", orderBy);

        return this.http.get<PaginationResponse<User>>(this.usersApiUrl, { headers: this.getAuthHeaders(), params, }).pipe(
            catchError(error => this.handleError(error, 'fetching all users'))
        );
    }

    getUserById(userId: number): Observable<User> {
        return this.http.get<User>(`${this.usersApiUrl}/${userId}`, { headers: this.getAuthHeaders() }).pipe(
            catchError(error => this.handleError(error, `fetching user with ID ${userId}`))
        );
    }

    getUserByUserName(userName: string): Observable<User> {
        return this.http.get<User>(`${this.usersApiUrl}/by-username/${userName}`, { headers: this.getAuthHeaders() }).pipe(
            catchError(error => this.handleError(error, `fetching user with username ${userName}`))
        );
    }


    updateUser(userId: number, userData: UpdateUserRequest): Observable<User> {
        return this.http.put<User>(`${this.usersApiUrl}/${userId}`, userData, { headers: this.getAuthHeaders() }).pipe(
            catchError(error => this.handleError(error, 'updating user'))
        );
    }

    updateContactDetails(userData: UpdateContactDetailsRequest): Observable<User> {
        return this.http.put<User>(`${this.usersApiUrl}`, userData, { headers: this.getAuthHeaders() }).pipe(
            catchError(error => this.handleError(error, 'updating user'))
        );
    }

    uploadProfilePicture(userId: number, file: File): Observable<any> {
        const formData: FormData = new FormData();
        formData.append('file', file, file.name);

        return this.http.put(`${this.usersApiUrl}/profile-picture`, formData, { headers: this.getAuthHeaders(true) }).pipe(
            catchError(error => this.handleError(error, 'uploading profile picture'))
        );
    }


    deleteUser(userId: number): Observable<User> {
        return this.http.delete<User>(`${this.usersApiUrl}/${userId}`, { headers: this.getAuthHeaders() }).pipe(
            catchError(error => this.handleError(error, `deleting user with ID ${userId}`))
        );
    }

    changePassword(passwordData: ChangePasswordRequest): Observable<User> {
        return this.http.put<User>(`${this.usersApiUrl}/change-password`, passwordData, { headers: this.getAuthHeaders() }).pipe(
            catchError(error => this.handleError(error, 'changing user password'))
        );
    }

    getAllRoles(): Observable<Role[]> {
        return this.http.get<Role[]>(this.rolesApiUrl, { headers: this.getAuthHeaders() }).pipe(
            catchError(error => this.handleError(error, 'fetching all roles'))
        );
    }

    getAdmins(): Observable<UserDetails[]> {
        return this.getAllUsers().pipe(
            map(response => response.data.filter(user => user.roleName === 'Admin'))
        );
    }

    getManagers(): Observable<UserDetails[]> {
        return this.getAllUsers().pipe(
            map(response => response.data.filter(user => user.roleName === 'Manager'))
        );
    }

}
