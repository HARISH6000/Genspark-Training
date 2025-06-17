import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms'; 
import { CommonModule, NgIf } from '@angular/common';
import { AuthService } from '../auth';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule, NgIf], 
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  username = ''; 
  password = ''; 
  errorMessage: string | null = null; 
  loading: boolean = false;

  constructor(private authService: AuthService, private router: Router) { }

  
  onLogin(): void {
    this.errorMessage = null;
    this.loading = true;

    this.authService.login(this.username, this.password).subscribe({
      next: () => {
        this.loading = false; 
        this.router.navigate(['/products']);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = 'Login failed. Please check your credentials.';
        console.error('Login error:', err);
      }
    });
  }
}

