import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { UserService } from '../../services/user.service';
import { RegisterUserRequest } from '../../models/user';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register-user',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register-user.html',
  styleUrl: './register-user.css'
})
export class RegisterUserComponent implements OnInit {
  registerRequest: RegisterUserRequest = {
    username: '',
    password: '',
    email: '',
    phone: '',
    roleId: 0
  };
  loading = false;
  successMessage = '';
  errorMessage = '';

  roles: { id: number, name: string }[] = [
    { id: 1, name: 'Admin' },
    { id: 2, name: 'Manager' }
  ];

  formSubmitted = false;

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
  }

  isFormValid(): boolean {
    const { username, password, email, phone, roleId } = this.registerRequest;
    return !!username && !!password && !!email && !!phone && roleId > 0 && this.isValidEmail(email);
  }

  isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  onRegister(): void {
    this.formSubmitted = true;
    this.successMessage = '';
    this.errorMessage = '';

    if (!this.isFormValid()) {
      this.errorMessage = 'Please fill in all required fields and ensure the email is valid.';
      return;
    }

    this.loading = true;

    this.userService.registerUser(this.registerRequest).pipe( 
      catchError(error => {
        this.loading = false;
        this.errorMessage = error.message || 'An unexpected error occurred during registration.';
        return of(null);
      })
    ).subscribe(response => {
      this.loading = false;
      if (response) {
        this.successMessage = `User "${response.username}" registered successfully with role "${response.roleName}".`;
        this.registerRequest = { username: '', password: '', email: '', phone: '', roleId: 0 };
        this.formSubmitted = false;
        this.router.navigate(['/user-info'], { state: { user: response } });
      }
    });
  }
}