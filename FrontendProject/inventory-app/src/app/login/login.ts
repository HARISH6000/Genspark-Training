import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { AuthService, LoginRequest } from '../services/auth.service'; 
import { Router } from '@angular/router'; 

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  loginRequest: LoginRequest = {
    username: '',
    password: ''
  };
  loading = false;
  error = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onLogin(): void {
    this.loading = true;
    this.error = '';

    this.authService.login(this.loginRequest).subscribe({
      next: (loginResponse) => {
        const username = loginResponse.username;
        this.authService.getUserDetails(username).subscribe({
          next: (userDetails) => {
            this.loading = false;
            if (userDetails.roleName.toLowerCase() === 'admin') {
              this.router.navigate(['/admin/dashboard']);
            } else if (userDetails.roleName.toLowerCase() === 'manager') {
              this.router.navigate(['/manager/dashboard']);
            } else {
              this.router.navigate(['/dashboard']);
            }
          },
          error: (err) => {
            this.loading = false;
            this.error = err.message || 'Failed to retrieve user details. Please try again.';
            this.authService.logout();
          }
        });
      },
      error: (err) => {
        this.loading = false;
        this.error = err.message || 'Invalid username or password.';
      }
    });
  }
}