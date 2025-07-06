import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { RegisterUserComponent } from './register-user';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { RegisterUserRequest, User } from '../../models/user';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

describe('RegisterUserComponent', () => {
  let component: RegisterUserComponent;
  let fixture: ComponentFixture<RegisterUserComponent>;
  let mockUserService: jasmine.SpyObj<UserService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockRegisteredUser: User = {
    userId: 1,
    username: 'testuser',
    email: 'test@example.com',
    phone: '1234567890',
    profilePictureUrl: '',
    roleId: 2,
    roleName: 'Manager',
    isDeleted: false,
  };

  beforeEach(async () => {
    mockUserService = jasmine.createSpyObj('UserService', ['registerUser']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    mockUserService.registerUser.and.returnValue(of(mockRegisteredUser));

    await TestBed.configureTestingModule({
      imports: [
        RegisterUserComponent,
        CommonModule,
        FormsModule,
      ],
      providers: [
        { provide: UserService, useValue: mockUserService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterUserComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should register a user successfully and navigate to user-info', fakeAsync(() => {
    const registerRequestData: RegisterUserRequest = {
      username: 'newuser',
      password: 'password123',
      email: 'newuser@example.com',
      phone: '1112223333',
      roleId: 2,
    };
    component.registerRequest = { ...registerRequestData };
    fixture.detectChanges();

    component.onRegister();
    tick();

    expect(mockUserService.registerUser).toHaveBeenCalledWith(registerRequestData);
    expect(component.loading).toBeFalse();
    expect(component.successMessage).toBe(`User "testuser" registered successfully with role "Manager".`);
    expect(component.registerRequest.username).toBe('');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/user-info'], { state: { user: mockRegisteredUser } });
  }));

  it('should display an error message if registration fails', fakeAsync(() => {
    const errorMessage = 'Username already exists.';
    mockUserService.registerUser.and.returnValue(throwError(() => new Error(errorMessage)));

    component.registerRequest = {
      username: 'existinguser',
      password: 'password123',
      email: 'existing@example.com',
      phone: '1112223333',
      roleId: 2,
    };
    fixture.detectChanges();

    component.onRegister();
    tick();

    expect(mockUserService.registerUser).toHaveBeenCalledWith(component.registerRequest);
    expect(component.loading).toBeFalse();
    expect(component.errorMessage).toBe(errorMessage);
    expect(component.successMessage).toBe('');
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  }));
});
