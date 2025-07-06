import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ManagerDashboardComponent } from './dashboard';
import { Router } from '@angular/router';
import { AuthService, UserDetails } from '../../services/auth.service';
import { InventoryService } from '../../services/inventory.service';
import { ProductService } from '../../services/product.service';
import { SignalrService } from '../../services/signalr.service';
import { NotificationService } from '../../services/notification.service';
import { of, BehaviorSubject, throwError } from 'rxjs';
import { Inventory } from '../../models/inventory';
import { Product } from '../../models/product';
import { LowStockNotificationDto } from '../../models/notification';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { InventoryProductPieChartComponent } from '../../shared/inventory-product-pie-chart/inventory-product-pie-chart';

interface MockAuthService extends jasmine.SpyObj<AuthService> {
  currentUser: BehaviorSubject<UserDetails | null>;
  currentUserValue: UserDetails | null;
}

describe('ManagerDashboardComponent', () => {
  let component: ManagerDashboardComponent;
  let fixture: ComponentFixture<ManagerDashboardComponent>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockAuthService: MockAuthService;
  let mockInventoryService: jasmine.SpyObj<InventoryService>;
  let mockProductService: jasmine.SpyObj<ProductService>;
  let mockSignalrService: jasmine.SpyObj<SignalrService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;

  const mockManagerUser: UserDetails = {
    userId: 1, username: 'manager', email: 'm@m.com', phone: '123', profilePictureUrl: '', roleId: 2, roleName: 'Manager', isDeleted: false
  };

  const mockInventories: Inventory[] = [
    { inventoryId: 101, name: 'Warehouse A', location: 'Loc A' },
    { inventoryId: 102, name: 'Warehouse B', location: 'Loc B' },
  ];

  const mockProducts: Product[] = [
    { productId: 1, sku: 'PROD001', productName: 'Laptop', description: 'Desc 1', unitPrice: 1200, categoryId: 1, categoryName: 'Electronics', isDeleted: false },
    { productId: 2, sku: 'PROD002', productName: 'Mouse', description: 'Desc 2', unitPrice: 25, categoryId: 1, categoryName: 'Electronics', isDeleted: false },
  ];

  const mockLowStockNotifications: LowStockNotificationDto[] = [
    { productId: 501, productName: 'Pen', sku: 'PEN001', currentQuantity: 2, minStockQuantity: 10, inventoryId: 101, inventoryName: 'Whse A', message: 'Pen is low in stock', timestamp: new Date().toISOString() },
  ];

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockAuthService = jasmine.createSpyObj<AuthService>('AuthService', ['isAdmin', 'isManager', 'currentUserValue']) as MockAuthService;
    mockInventoryService = jasmine.createSpyObj('InventoryService', ['getInventoriesByManagerId', 'getProductsInInventory']);
    mockProductService = jasmine.createSpyObj('ProductService', ['getAllProducts']);
    mockSignalrService = jasmine.createSpyObj('SignalrService', ['connectionStatus$']);
    mockNotificationService = jasmine.createSpyObj('NotificationService', ['lowStockNotification$', 'setLowStockNotification']);

    mockAuthService.currentUserValue = mockManagerUser;
    mockAuthService.currentUser = new BehaviorSubject<UserDetails | null>(mockManagerUser);
    mockAuthService.isAdmin.and.returnValue(false);
    mockAuthService.isManager.and.returnValue(true);

    mockInventoryService.getInventoriesByManagerId.and.returnValue(of(mockInventories));
    mockInventoryService.getProductsInInventory.and.returnValue(of({
      data: [{ productId: 1, quantityInInventory: 10, minStockQuantity: 5 } as any],
      pagination: { totalRecords: 1, page: 1, pageSize: 1, totalPages: 1 }
    }));
    mockProductService.getAllProducts.and.returnValue(of({
      data: mockProducts,
      pagination: { totalRecords: 2, page: 1, pageSize: 4, totalPages: 1 }
    }));
    mockSignalrService.connectionStatus$ = new BehaviorSubject<boolean>(true).asObservable();
    mockNotificationService.lowStockNotification$ = new BehaviorSubject<LowStockNotificationDto[]>(mockLowStockNotifications).asObservable();

    await TestBed.configureTestingModule({
      imports: [
        ManagerDashboardComponent,
        CommonModule,
        InventoryProductPieChartComponent,
        CurrencyPipe
      ],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthService, useValue: mockAuthService },
        { provide: InventoryService, useValue: mockInventoryService },
        { provide: ProductService, useValue: mockProductService },
        { provide: SignalrService, useValue: mockSignalrService },
        { provide: NotificationService, useValue: mockNotificationService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ManagerDashboardComponent);
    component = fixture.componentInstance;
  });

  it('should create and load dashboard data on ngOnInit for a manager', fakeAsync(() => {
    fixture.detectChanges();
    tick(1);
    tick(100);

    expect(component).toBeTruthy();
    expect(component.loading).toBeFalse();
    expect(component.currentUser).toEqual(mockManagerUser);
    expect(mockInventoryService.getInventoriesByManagerId).toHaveBeenCalledWith(mockManagerUser.userId);
    expect(mockProductService.getAllProducts).toHaveBeenCalledWith(1, 4, null, null, false);
    expect(component.managedInventories).toEqual(mockInventories);
    expect(component.featuredProducts).toEqual(mockProducts);
    expect(component.lowStockNotifications).toEqual(mockLowStockNotifications);
    expect(component.signalRConnectionStatus).toBe('Connected');
  }));

  it('should navigate to notifications page when viewNotifications is called', () => {
    component.viewNotifications();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/notifications']);
  });

  it('should navigate to inventory details page when viewInventoryDetails is called', () => {
    const inventoryId = 101;
    component.viewInventoryDetails(inventoryId);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/inventory', inventoryId]);
  });

  it('should display error message if dashboard data fails to load', fakeAsync(() => {
    mockInventoryService.getInventoriesByManagerId.and.returnValue(throwError(() => new Error('Failed to fetch inventories')));
    fixture.detectChanges();
    tick(1);
    tick(100);

    expect(component.loading).toBeFalse();
    expect(component.errorMessage).toBe('Failed to fetch inventories');
    expect(component.managedInventories).toEqual([]);
    expect(component.featuredProducts).toEqual([]);
    expect(component.inventoryProductCounts).toEqual([]);
  }));
});
