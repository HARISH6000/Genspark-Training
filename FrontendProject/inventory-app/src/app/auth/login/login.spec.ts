import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { LoginComponent } from './login';
import { AuthService, LoginResponse, UserDetails } from '../../services/auth.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockLoginResponse: LoginResponse = {
    userId: 1,
    username: 'testuser',
    token: 'mock-token',
    refreshToken: 'mock-refresh-token',
  };

  const mockAdminUserDetails: UserDetails = {
    userId: 1,
    username: 'adminuser',
    email: 'admin@example.com',
    phone: '12345',
    profilePictureUrl: '',
    roleId: 1,
    roleName: 'Admin',
    isDeleted: false,
  };

  const mockManagerUserDetails: UserDetails = {
    userId: 2,
    username: 'manageruser',
    email: 'manager@example.com',
    phone: '67890',
    profilePictureUrl: '',
    roleId: 2,
    roleName: 'Manager',
    isDeleted: false,
  };

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj('AuthService', ['login', 'getUserDetails', 'logout']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    mockAuthService.login.and.returnValue(of(mockLoginResponse));
    mockAuthService.getUserDetails.and.returnValue(of(mockAdminUserDetails));

    await TestBed.configureTestingModule({
      imports: [
        LoginComponent,
        CommonModule,
        FormsModule,
      ],
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should log in successfully and navigate to admin dashboard if user is admin', fakeAsync(() => {
    component.loginRequest = { username: 'adminuser', password: 'password' };
    mockAuthService.getUserDetails.and.returnValue(of(mockAdminUserDetails));

    component.onLogin();
    tick();
    tick();

    expect(mockAuthService.login).toHaveBeenCalledWith(component.loginRequest);
    expect(mockAuthService.getUserDetails).toHaveBeenCalledWith('testuser');
    expect(component.loading).toBeFalse();
    expect(component.error).toBe('');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/admin/dashboard']);
  }));

  it('should display error message if login fails', fakeAsync(() => {
    const errorMessage = 'Invalid credentials.';
    mockAuthService.login.and.returnValue(throwError(() => new Error(errorMessage)));

    component.loginRequest = { username: 'baduser', password: 'badpassword' };

    component.onLogin();
    tick();

    expect(mockAuthService.login).toHaveBeenCalledWith(component.loginRequest);
    expect(component.loading).toBeFalse();
    expect(component.error).toBe(errorMessage);
    expect(mockAuthService.getUserDetails).not.toHaveBeenCalled();
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  }));
});
