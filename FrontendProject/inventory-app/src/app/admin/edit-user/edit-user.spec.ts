import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { EditUserComponent } from './edit-user';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, UserDetails } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { User, Role, UpdateUserRequest } from '../../models/user';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface MockAuthService extends jasmine.SpyObj<AuthService> {
  currentUser: BehaviorSubject<UserDetails | null>;
  currentUserValue: UserDetails | null;
}

describe('EditUserComponent', () => {
  let component: EditUserComponent;
  let fixture: ComponentFixture<EditUserComponent>;
  let mockActivatedRoute: { paramMap: BehaviorSubject<Map<string, string>> };
  let mockRouter: jasmine.SpyObj<Router>;
  let mockAuthService: MockAuthService;
  let mockUserService: jasmine.SpyObj<UserService>;

  const mockUser: User = {
    userId: 1,
    username: 'testuser',
    email: 'test@example.com',
    phone: '1234567890',
    profilePictureUrl: 'http://example.com/pic.jpg',
    roleId: 2,
    roleName: 'Manager',
    isDeleted: false,
  };

  const mockAdminUser: UserDetails = {
    userId: 99, username: 'admin', email: 'admin@example.com', phone: '999', profilePictureUrl: '', roleId: 1, roleName: 'Admin', isDeleted: false
  };

  const mockRoles: Role[] = [
    { roleId: 1, roleName: 'Admin', description: 'Administrator' },
    { roleId: 2, roleName: 'Manager', description: 'Manager' },
  ];

  beforeEach(async () => {
    mockActivatedRoute = {
      paramMap: new BehaviorSubject(new Map([['userId', '1']]))
    };
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockAuthService = jasmine.createSpyObj<AuthService>('AuthService', ['isAdmin', 'currentUserValue']) as MockAuthService;
    mockUserService = jasmine.createSpyObj('UserService', ['getUserById', 'updateUser', 'uploadProfilePicture', 'getAllRoles']);

    mockAuthService.currentUserValue = { ...mockUser, roleName: 'Manager' };
    mockAuthService.currentUser = new BehaviorSubject<UserDetails | null>(mockAuthService.currentUserValue);
    mockAuthService.isAdmin.and.returnValue(false);
    mockUserService.getUserById.and.returnValue(of(mockUser));
    mockUserService.updateUser.and.returnValue(of(mockUser));
    mockUserService.uploadProfilePicture.and.returnValue(of({} as any));
    mockUserService.getAllRoles.and.returnValue(of(mockRoles));

    await TestBed.configureTestingModule({
      imports: [
        EditUserComponent,
        CommonModule,
        FormsModule,
      ],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        { provide: AuthService, useValue: mockAuthService },
        { provide: UserService, useValue: mockUserService },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EditUserComponent);
    component = fixture.componentInstance;
  });

  it('should create and load user details on ngOnInit', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    expect(component).toBeTruthy();
    expect(component.editingUserId).toBe(1);
    expect(component.userToEdit).toEqual(mockUser);
    expect(mockUserService.getUserById).toHaveBeenCalledWith(1);
    expect(component.loading).toBeFalse();
  }));

  it('should update user details successfully when onSubmit is called', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    component.userToEdit = { ...mockUser, email: 'updated@example.com' };
    
    component.onSubmit();
    tick();

    const expectedUpdateRequest: UpdateUserRequest = {
      username: mockUser.username,
      email: 'updated@example.com',
      phone: mockUser.phone,
      roleId: mockUser.roleId,
    };

    expect(component.saving).toBeFalse();
    expect(mockUserService.updateUser).toHaveBeenCalledWith(1, expectedUpdateRequest);
    expect(component.successMessage).toBe('User details updated successfully!');
    expect(component.errorMessage).toBeNull();
  }));

  it('should upload profile picture successfully', fakeAsync(() => {
    component.editingUserId = 1;
    component.userToEdit = mockUser;
    component.isEditingOwnProfile = true;
    fixture.detectChanges();

    const mockFile = new File(['dummy content'], 'profile.png', { type: 'image/png' });
    component.selectedFile = mockFile;

    component.uploadProfilePicture();
    tick();

    expect(component.uploadingPicture).toBeFalse();
    expect(mockUserService.uploadProfilePicture).toHaveBeenCalledWith(1, mockFile);
    expect(component.successMessage).toBe('Profile picture uploaded successfully!');
    expect(component.errorMessage).toBeNull();
    expect(component.selectedFile).toBeNull();
    expect(mockUserService.getUserById).toHaveBeenCalledWith(1);
  }));
});
