import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { UserInfoComponent } from './user-info';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, UserDetails } from '../../services/auth.service';
import { InventoryService } from '../../services/inventory.service';
import { UserService } from '../../services/user.service';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { Inventory } from '../../models/inventory';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface MockAuthService extends jasmine.SpyObj<AuthService> {
  currentUser: BehaviorSubject<UserDetails | null>;
  currentUserValue: UserDetails | null;
}

describe('UserInfoComponent', () => {
  let component: UserInfoComponent;
  let fixture: ComponentFixture<UserInfoComponent>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: any;
  let mockInventoryService: jasmine.SpyObj<InventoryService>;
  let mockAuthService: MockAuthService;
  let mockUserService: jasmine.SpyObj<UserService>;

  const mockUser: UserDetails = {
    userId: 1,
    username: 'testuser',
    email: 'test@example.com',
    phone: '1234567890',
    profilePictureUrl: 'http://example.com/profile.jpg',
    roleId: 2,
    roleName: 'Manager',
    isDeleted: false,
  };

  const mockInventories: Inventory[] = [
    { inventoryId: 101, name: 'Warehouse A', location: 'Location A' },
    { inventoryId: 102, name: 'Warehouse B', location: 'Location B' },
  ];

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate', 'getCurrentNavigation']);
    mockActivatedRoute = {
      paramMap: of(new Map([['userId', '1']]))
    };
    mockInventoryService = jasmine.createSpyObj('InventoryService', ['getInventoriesByManagerId']);
    mockAuthService = jasmine.createSpyObj<AuthService>('AuthService', ['isAdmin', 'currentUserValue']) as MockAuthService;
    mockUserService = jasmine.createSpyObj('UserService', ['deleteUser']);

    mockInventoryService.getInventoriesByManagerId.and.returnValue(of(mockInventories));
    mockAuthService.isAdmin.and.returnValue(false);
    mockAuthService.currentUserValue = null;
    mockAuthService.currentUser = new BehaviorSubject<UserDetails | null>(null);
    mockRouter.getCurrentNavigation.and.returnValue({
      extras: {
        state: {
          user: mockUser
        }
      }
    } as any);

    await TestBed.configureTestingModule({
      imports: [
        UserInfoComponent,
        CommonModule,
        FormsModule,
      ],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: InventoryService, useValue: mockInventoryService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: UserService, useValue: mockUserService },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserInfoComponent);
    component = fixture.componentInstance;
  });

  it('should create and load user and inventories from router state on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    expect(component).toBeTruthy();
    expect(component.user).toEqual(mockUser);
    expect(component.inventories).toEqual(mockInventories);
    expect(component.filteredInventories).toEqual(mockInventories);
    expect(component.loading).toBeFalse();
    expect(mockInventoryService.getInventoriesByManagerId).toHaveBeenCalledWith(mockUser.userId, '');
  }));

  it('should filter inventories by location', fakeAsync(() => {
    component.user = mockUser;
    component.inventories = mockInventories;
    component.filteredInventories = mockInventories;
    fixture.detectChanges();
    tick();

    component.locationFilter = 'Location A';
    component.onFilterChange();
    fixture.detectChanges();

    expect(component.filteredInventories.length).toBe(1);
    expect(component.filteredInventories[0]).toEqual(mockInventories[0]);
  }));

  it('should navigate to edit user page when editUser is called', () => {
    component.user = mockUser;
    component.editUser();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/edit-user', mockUser.userId]);
  });
});
