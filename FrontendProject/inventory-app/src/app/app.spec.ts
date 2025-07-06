import { TestBed, ComponentFixture } from '@angular/core/testing';
import { App } from './app';
import { SignalrService } from './services/signalr.service';
import { NotificationService } from './services/notification.service';
import { AuthService } from './services/auth.service';
import { InventoryService } from './services/inventory.service';
import { of, throwError, Subscription, BehaviorSubject } from 'rxjs'; // Import BehaviorSubject
import { LowStockNotificationDto } from './models/notification';
import { Inventory } from './models/inventory';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './shared/navbar/navbar';
import { provideRouter } from '@angular/router'; // Import provideRouter

describe('App', () => {
  let fixture: ComponentFixture<App>;
  let component: App;
  let mockSignalrService: jasmine.SpyObj<SignalrService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockInventoryService: jasmine.SpyObj<InventoryService>;

  const mockLowStockNotification: LowStockNotificationDto = {
    productId: 101, // Added based on LowStockNotificationDto
    productName: 'Test Product',
    sku: 'SKU001', // Added based on LowStockNotificationDto
    currentQuantity: 5, // Changed from currentStock to currentQuantity
    minStockQuantity: 10, // Changed from threshold to minStockQuantity
    inventoryId: 1,
    inventoryName: 'Main Warehouse', // Added based on LowStockNotificationDto
    message: 'Product is low on stock.', // Added based on LowStockNotificationDto
    timestamp: new Date().toISOString()
  };

  const mockInventory: Inventory = {
    inventoryId: 1,
    name: 'Warehouse A', // Changed from productName
    location: 'Location A', // Changed from location
    // Removed properties not in Inventory interface:
    // currentStock, reorderLevel, lastUpdated, managerId
  };

  beforeEach(async () => {
    // Create spy objects for services
    // FIX: Add 'currentUser' to the spy object for AuthService
    mockSignalrService = jasmine.createSpyObj('SignalrService', ['startConnection', 'lowStockNotifications$']);
    mockNotificationService = jasmine.createSpyObj('NotificationService', ['setLowStockNotification']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['isAdmin', 'currentUserId', 'currentUser', 'currentUserValue']); // Added currentUser and currentUserValue
    mockInventoryService = jasmine.createSpyObj('InventoryService', ['getInventoriesByManagerId']);

    // Set up initial mock values for service methods
    mockSignalrService.startConnection.and.returnValue(Promise.resolve());
    mockSignalrService.lowStockNotifications$ = of(mockLowStockNotification);
    mockInventoryService.getInventoriesByManagerId.and.returnValue(of([]));

    // FIX: Provide a mock BehaviorSubject for currentUser
    mockAuthService.currentUser = new BehaviorSubject(null); // Initialize with null or a mock UserDetails object
    // FIX: Provide a mock value for currentUserValue getter
    Object.defineProperty(mockAuthService, 'currentUserValue', {
      configurable: true,
      get: () => null // Default to null, can be overridden in specific tests
    });

    Object.defineProperty(mockAuthService, 'currentUserId', {
      configurable: true,
      get: () => 123 // Default user ID for tests
    });


    await TestBed.configureTestingModule({
      imports: [
        App, // Import the standalone component directly
        RouterOutlet, // Required if your component uses <router-outlet>
        CommonModule, // Required for common directives like ngIf, ngFor
        NavbarComponent // Import any child components used in the template
      ],
      providers: [
        // Provide the mock services
        { provide: SignalrService, useValue: mockSignalrService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: InventoryService, useValue: mockInventoryService },
        provideRouter([]) // FIX: Provide a mock router for testing
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    component = fixture.componentInstance;

    // Clear session storage before each test to ensure a clean state
    sessionStorage.clear();
  });

  it('should create the app', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize title', () => {
    expect(component.title).toEqual('inventory-app');
  });

  describe('ngOnInit', () => {
    it('should start SignalR connection on ngOnInit', () => {
      component.ngOnInit();
      expect(mockSignalrService.startConnection).toHaveBeenCalled();
    });

    it('should load notifications from session storage if available', () => {
      const storedNotifications: LowStockNotificationDto[] = [{
        productId: 102,
        productName: 'Stored Product',
        sku: 'SKU002',
        currentQuantity: 1,
        minStockQuantity: 2,
        inventoryId: 2,
        inventoryName: 'Secondary Warehouse',
        message: 'Another product low.',
        timestamp: new Date().toISOString()
      }];
      sessionStorage.setItem('lowStockNotifications', JSON.stringify(storedNotifications));

      component.ngOnInit();
      expect(component.notifications).toEqual(storedNotifications);
      expect(mockNotificationService.setLowStockNotification).toHaveBeenCalledWith(storedNotifications);
    });

    it('should initialize with empty notifications if session storage is empty', () => {
      component.ngOnInit();
      expect(component.notifications).toEqual([]);
      expect(mockNotificationService.setLowStockNotification).toHaveBeenCalledWith([]);
    });

    
    
  });

  describe('ngOnDestroy', () => {
    it('should unsubscribe from notifications and connection status on ngOnDestroy', () => {
      // Create mock subscriptions and spy on their individual unsubscribe methods
      const mockNotificationsSub = new Subscription();
      const mockConnectionStatusSub = new Subscription();
      
      const notificationsSubSpy = spyOn(mockNotificationsSub, 'unsubscribe');
      const connectionStatusSubSpy = spyOn(mockConnectionStatusSub, 'unsubscribe');

      // Assign the mock subscriptions to the component properties
      (component as any)['notificationsSub'] = mockNotificationsSub;
      (component as any)['connectionStatusSub'] = mockConnectionStatusSub;

      component.ngOnDestroy();

      // Expect unsubscribe to have been called once for each mock subscription
      expect(notificationsSubSpy).toHaveBeenCalledTimes(1);
      expect(connectionStatusSubSpy).toHaveBeenCalledTimes(1);
    });
  });
});
