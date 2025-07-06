import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NavbarComponent } from './navbar';
import { AuthService, UserDetails } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, provideRouter } from '@angular/router';
import { BehaviorSubject } from 'rxjs';

interface MockAuthService extends jasmine.SpyObj<AuthService> {
  currentUser: BehaviorSubject<UserDetails | null>;
  currentUserValue: UserDetails | null;
}

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let mockAuthService: MockAuthService;

  const mockAdminUser: UserDetails = {
    userId: 1,
    username: 'adminUser',
    email: 'admin@example.com',
    phone: '123-456-7890',
    profilePictureUrl: '',
    roleId: 1,
    roleName: 'Admin',
    isDeleted: false,
  };

  const mockManagerUser: UserDetails = {
    userId: 2,
    username: 'managerUser',
    email: 'manager@example.com',
    phone: '098-765-4321',
    profilePictureUrl: '',
    roleId: 2,
    roleName: 'Manager',
    isDeleted: false,
  };

  const mockRegularUser: UserDetails = {
    userId: 3,
    username: 'regularUser',
    email: 'user@example.com',
    phone: '111-222-3333',
    profilePictureUrl: '',
    roleId: 3,
    roleName: 'User',
    isDeleted: false,
  };

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj<AuthService>('AuthService', [
      'isAdmin',
      'isManager',
      'logout',
    ]) as MockAuthService;

    mockAuthService.currentUser = new BehaviorSubject<UserDetails | null>(null);

    Object.defineProperty(mockAuthService, 'currentUserValue', {
      configurable: true,
      get: () => mockAuthService.currentUser.value
    });

    await TestBed.configureTestingModule({
      imports: [
        NavbarComponent,
        CommonModule,
        RouterLink,
        RouterLinkActive
      ],
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should update currentUser on authService.currentUser changes', () => {
    expect(component.currentUser).toBeNull();
    mockAuthService.currentUser.next(mockAdminUser);
    fixture.detectChanges();
    expect(component.currentUser).toEqual(mockAdminUser);
    mockAuthService.currentUser.next(null);
    fixture.detectChanges();
    expect(component.currentUser).toBeNull();
  });

  it('should return true for isLoggedIn if currentUserValue is present', () => {
    mockAuthService.currentUser.next(mockRegularUser);
    expect(component.isLoggedIn()).toBeTrue();
  });

  it('should return false for isLoggedIn if currentUserValue is null', () => {
    mockAuthService.currentUser.next(null);
    expect(component.isLoggedIn()).toBeFalse();
  });

  it('should call authService.isAdmin when isAdmin is called', () => {
    mockAuthService.isAdmin.and.returnValue(true);
    expect(component.isAdmin()).toBeTrue();
    expect(mockAuthService.isAdmin).toHaveBeenCalled();
  });

  it('should call authService.isManager when isManager is called', () => {
    mockAuthService.isManager.and.returnValue(true);
    expect(component.isManager()).toBeTrue();
    expect(mockAuthService.isManager).toHaveBeenCalled();
  });

  it('should call authService.logout when logout is called', () => {
    component.logout();
    expect(mockAuthService.logout).toHaveBeenCalled();
  });

  it('should unsubscribe on ngOnDestroy', () => {
    component.ngOnInit();
    const unsubscribeSpy = spyOn((component as any)['userSubscription'], 'unsubscribe');
    component.ngOnDestroy();
    expect(unsubscribeSpy).toHaveBeenCalled();
  });
});
