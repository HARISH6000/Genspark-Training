import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { UserManagementComponent } from './user-management';
import { Router } from '@angular/router';
import { AuthService, UserDetails } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { of, BehaviorSubject } from 'rxjs';
import { User, Role } from '../../models/user';
import { PaginationResponse } from '../../models/pagination-response';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageNumberComponent } from '../../shared/page-number/page-number';

interface MockAuthService extends jasmine.SpyObj<AuthService> {
  currentUser: BehaviorSubject<UserDetails | null>;
  currentUserValue: UserDetails | null;
}

describe('UserManagementComponent', () => {
  let component: UserManagementComponent;
  let fixture: ComponentFixture<UserManagementComponent>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockAuthService: MockAuthService;
  let mockUserService: jasmine.SpyObj<UserService>;

  const mockAdminUser: UserDetails = {
    userId: 1, username: 'admin', email: 'admin@example.com', phone: '123', profilePictureUrl: '', roleId: 1, roleName: 'Admin', isDeleted: false
  };

  const mockUsers: User[] = [
    { userId: 1, username: 'admin', email: 'admin@example.com', phone: '111', profilePictureUrl: '', roleId: 1, roleName: 'Admin', isDeleted: false },
    { userId: 2, username: 'manager', email: 'manager@example.com', phone: '222', profilePictureUrl: '', roleId: 2, roleName: 'Manager', isDeleted: false },
    { userId: 3, username: 'user', email: 'user@example.com', phone: '333', profilePictureUrl: '', roleId: 3, roleName: 'User', isDeleted: false },
  ];

  const mockRoles: Role[] = [
    { roleId: 1, roleName: 'Admin', description: 'Admin Role' },
    { roleId: 2, roleName: 'Manager', description: 'Manager Role' },
    { roleId: 3, roleName: 'User', description: 'Regular User Role' },
  ];

  const mockPaginationResponse: PaginationResponse<User> = {
    data: mockUsers,
    pagination: {
      totalRecords: 3,
      page: 1,
      pageSize: 5,
      totalPages: 1,
    },
  };

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockAuthService = jasmine.createSpyObj<AuthService>('AuthService', ['isAdmin', 'currentUserValue']) as MockAuthService;
    mockUserService = jasmine.createSpyObj('UserService', ['getAllUsers', 'getAllRoles', 'getUserById', 'getUserByUserName']);

    mockAuthService.isAdmin.and.returnValue(true);
    mockAuthService.currentUserValue = mockAdminUser;
    mockAuthService.currentUser = new BehaviorSubject<UserDetails | null>(mockAdminUser);

    mockUserService.getAllUsers.and.returnValue(of(mockPaginationResponse));
    mockUserService.getAllRoles.and.returnValue(of(mockRoles));
    mockUserService.getUserById.and.returnValue(of(mockUsers[0]));
    mockUserService.getUserByUserName.and.returnValue(of(mockUsers[0]));

    await TestBed.configureTestingModule({
      imports: [
        UserManagementComponent,
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        PageNumberComponent,
      ],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthService, useValue: mockAuthService },
        { provide: UserService, useValue: mockUserService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UserManagementComponent);
    component = fixture.componentInstance;
  });

  it('should create and fetch all users and roles on ngOnInit if user is admin', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);

    expect(component).toBeTruthy();
    expect(mockUserService.getAllRoles).toHaveBeenCalled();
    expect(component.roles).toEqual(mockRoles);
    expect(mockUserService.getAllUsers).toHaveBeenCalled();
    expect(component.users).toEqual(mockUsers);
    expect(component.filteredUsers).toEqual(mockUsers);
    expect(component.loading).toBeFalse();
  }));

  it('should filter users by selected role', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);

    component.selectedRole = 2;
    component.onRoleChange();
    fixture.detectChanges();

    expect(component.filteredUsers.length).toBe(1);
    expect(component.filteredUsers[0].roleName).toBe('Manager');
  }));
});
