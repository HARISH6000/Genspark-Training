import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { of, BehaviorSubject, throwError } from 'rxjs';
import { AdminDashboardComponent } from './dashboard';

import { AuthService, UserDetails } from '../../services/auth.service';
import { InventoryService } from '../../services/inventory.service';
import { ProductService } from '../../services/product.service';
import { SignalrService } from '../../services/signalr.service';
import { UserService } from '../../services/user.service';
import { CategoryService } from '../../services/category.service';
import { PaginationResponse } from '../../models/pagination-response';
import { Inventory, Product } from '../../models/inventory';
import { LowStockNotificationDto } from '../../models/notification';

class MockAuthService {
  private _currentUser = new BehaviorSubject<UserDetails | null>({
    userId: 1,
    username: 'testadmin',
    roleName: 'Admin',
    email: 'admin@example.com',
    phone: '123-456-7890',
    profilePictureUrl: 'http://example.com/pic.jpg',
    roleId: 1,
    isDeleted: false
  });
  currentUserValue = this._currentUser.value;
  currentUser$ = this._currentUser.asObservable();
  login = jasmine.createSpy('login').and.returnValue(of(true));
}

class MockInventoryService {
  getAllInventories = jasmine.createSpy('getAllInventories').and.returnValue(of({
    data: [
      { inventoryId: 1, name: 'Main Warehouse', location: 'New York' },
      { inventoryId: 2, name: 'Branch Store', location: 'Los Angeles' }
    ],
    pagination: {}
  } as PaginationResponse<Inventory>));
  getProductsInInventory = jasmine.createSpy('getProductsInInventory').and.returnValue(of({
    data: [{ productId: 1, productName: 'Laptop', quantityInInventory: 10 }],
    pagination: {}
  }));
  getManagersByInventoryId = jasmine.createSpy('getManagersByInventoryId').and.returnValue(of([
    { userId: 101, username: 'Manager1' }
  ]));
}

class MockProductService {
  getAllProducts = jasmine.createSpy('getAllProducts').and.returnValue(of({
    data: [
      { productId: 1, productName: 'Laptop', sku: 'LAP001', unitPrice: 1200, categoryName: 'Electronics' },
      { productId: 2, productName: 'Mouse', sku: 'MOU001', unitPrice: 25, categoryName: 'Electronics' }
    ],
    pagination: {}
  } as PaginationResponse<Product>));
}

class MockSignalrService {
  lowStockNotifications$ = new BehaviorSubject<LowStockNotificationDto[]>([]);
  connectionStatus$ = new BehaviorSubject<boolean>(true);
  startConnection = jasmine.createSpy('startConnection').and.returnValue(Promise.resolve());
  stopConnection = jasmine.createSpy('stopConnection');
}

class MockUserService {
  getAllUsers = jasmine.createSpy('getAllUsers').and.returnValue(of({
    data: [
      { userId: 1, username: 'testadmin', roleName: 'Admin', email: 'admin@example.com', phone: '111', profilePictureUrl: '', roleId: 1, isDeleted: false },
      { userId: 2, username: 'testuser', roleName: 'User', email: 'user@example.com', phone: '222', profilePictureUrl: '', roleId: 2, isDeleted: false }
    ],
    pagination: {}
  } as PaginationResponse<UserDetails>));
  getAdmins = jasmine.createSpy('getAdmins').and.returnValue(of([{ userId: 1, username: 'testadmin', phone: '111', profilePictureUrl: '', roleId: 1, isDeleted: false }]));
  getManagers = jasmine.createSpy('getManagers').and.returnValue(of([{ userId: 101, username: 'Manager1', phone: '333', profilePictureUrl: '', roleId: 3, isDeleted: false }]));
}

class MockCategoryService {
  getAllCategories = jasmine.createSpy('getAllCategories').and.returnValue(of([
    { categoryId: 1, categoryName: 'Electronics' },
    { categoryId: 2, categoryName: 'Office Supplies' }
  ]));
}

describe('AdminDashboardComponent', () => {
  let component: AdminDashboardComponent;
  let fixture: ComponentFixture<AdminDashboardComponent>;
  let mockAuthService: MockAuthService;
  let mockInventoryService: MockInventoryService;
  let mockProductService: MockProductService;
  let mockSignalrService: MockSignalrService;
  let mockUserService: MockUserService;
  let mockCategoryService: MockCategoryService;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminDashboardComponent],
      providers: [
        { provide: AuthService, useClass: MockAuthService },
        { provide: InventoryService, useClass: MockInventoryService },
        { provide: ProductService, useClass: MockProductService },
        { provide: SignalrService, useClass: MockSignalrService },
        { provide: UserService, useClass: MockUserService },
        { provide: CategoryService, useClass: MockCategoryService },
        { provide: Router, useValue: { navigate: jasmine.createSpy('navigate') } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminDashboardComponent);
    component = fixture.componentInstance;
    mockAuthService = TestBed.inject(AuthService) as unknown as MockAuthService;
    mockInventoryService = TestBed.inject(InventoryService) as unknown as MockInventoryService;
    mockProductService = TestBed.inject(ProductService) as unknown as MockProductService;
    mockSignalrService = TestBed.inject(SignalrService) as unknown as MockSignalrService;
    mockUserService = TestBed.inject(UserService) as unknown as MockUserService;
    mockCategoryService = TestBed.inject(CategoryService) as unknown as MockCategoryService;
    router = TestBed.inject(Router);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display loading spinner initially', () => {
    component.loading = true;
    fixture.detectChanges();
    const spinner = fixture.nativeElement.querySelector('.spinner-border');
    expect(spinner).toBeTruthy();
    expect(spinner.textContent).toContain('Loading dashboard...');
  });

  it('should display error message if data fetch fails', fakeAsync(() => {
    mockProductService.getAllProducts.and.returnValue(throwError(() => new Error('Failed to load dashboard data.')));
    component.ngOnInit();
    tick();
    fixture.detectChanges();
    const errorMessageElement = fixture.nativeElement.querySelector('.alert-danger');
    expect(errorMessageElement).toBeTruthy();
    expect(errorMessageElement.textContent).toContain('Failed to load dashboard data.');
    expect(component.loading).toBeFalse();
    expect(component.errorMessage).not.toBeNull();
  }));

  it('should display dashboard data after successful fetch', fakeAsync(() => {
    mockAuthService.currentUserValue = {
      userId: 1,
      username: 'testadmin',
      roleName: 'Admin',
      email: 'admin@example.com',
      phone: '123-456-7890',
      profilePictureUrl: 'http://example.com/pic.jpg',
      roleId: 1,
      isDeleted: false
    };
    mockSignalrService.startConnection.and.returnValue(Promise.resolve());
    mockSignalrService.connectionStatus$.next(true);
    component.ngOnInit();
    tick();
    tick();
    fixture.detectChanges();
    expect(component.totalProductsCount).toBe(2);
    expect(component.totalCategoriesCount).toBe(2);
    expect(component.totalInventoriesCount).toBe(2);
    expect(component.totalUsersCount).toBe(2);
    expect(component.totalAdminsCount).toBe(1);
    expect(component.totalManagersCount).toBe(1);
    expect(component.totalLocationsCount).toBe(2);
    const recentUsersCards = fixture.nativeElement.querySelectorAll('.recent-users .clickable-card');
    expect(component.loading).toBeFalse();
    expect(component.errorMessage).toBeNull();
  }));

  it('should show low stock notifications and connection status', fakeAsync(() => {
    mockSignalrService.connectionStatus$.next(true);
    fixture.detectChanges();
    let statusElement = fixture.nativeElement.querySelector('.notification-card span');
    expect(statusElement.textContent).toContain('Status: Connected');
    expect(statusElement.classList).toContain('text-success');
    const lowStockNotif: LowStockNotificationDto = {
      productId: 3,
      productName: 'Widget X',
      sku: 'WID001',
      message: 'Low stock alert!',
      currentQuantity: 5,
      minStockQuantity: 10,
      inventoryId: 101,
      inventoryName: 'Main Warehouse',
      timestamp: new Date().toISOString()
    };
    mockSignalrService.lowStockNotifications$.next([lowStockNotif]);
    fixture.detectChanges();
    const notificationItem = fixture.nativeElement.querySelector('.list-group-item');
    expect(notificationItem).toBeTruthy();
    const badge = notificationItem.querySelector('.badge.bg-danger');
    expect(badge).toBeTruthy();
  }));

  it('should filter pie chart data by location', fakeAsync(() => {
    component.allInventoryProductCounts = [
      { inventoryName: 'Warehouse A', totalProducts: 50, inventoryId: 1, location: 'New York' },
      { inventoryName: 'Warehouse B', totalProducts: 30, inventoryId: 2, location: 'Los Angeles' },
      { inventoryName: 'Warehouse C', totalProducts: 20, inventoryId: 3, location: 'New York' },
    ];
    component.availableLocations = ['All Locations', 'New York', 'Los Angeles'];
    fixture.detectChanges();
    const selectElement: HTMLSelectElement = fixture.nativeElement.querySelector('#locationFilterPie');
    selectElement.value = 'New York';
    selectElement.dispatchEvent(new Event('change'));
    tick();
    fixture.detectChanges();
    expect(component.pieChartSelectedLocation).toBe('New York');
    expect(component.filteredInventoryProductCounts.length).toBe(2);
    expect(component.filteredInventoryProductCounts.every(item => item.location === 'New York')).toBeTrue();
    selectElement.value = 'All Locations';
    selectElement.dispatchEvent(new Event('change'));
    tick();
    fixture.detectChanges();
    expect(component.pieChartSelectedLocation).toBe('All Locations');
    expect(component.filteredInventoryProductCounts.length).toBe(3);
  }));
});
