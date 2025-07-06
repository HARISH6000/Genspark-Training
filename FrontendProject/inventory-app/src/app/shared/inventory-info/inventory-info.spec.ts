import { ComponentFixture, TestBed, fakeAsync, tick, flush } from '@angular/core/testing';
import { InventoryInfoComponent } from './inventory-info';
import { ActivatedRoute, Router } from '@angular/router';
import { InventoryService } from '../../services/inventory.service';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { AuthService, UserDetails } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { Inventory, ProductsForInventories, ManagerUser, InventoryManagerAssignmentRequest } from '../../models/inventory';
import { Product } from '../../models/product';
import { PaginationResponse } from '../../models/pagination-response';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageNumberComponent } from '../../shared/page-number/page-number';
import { NgSelectModule } from '@ng-select/ng-select';

interface MockAuthService extends jasmine.SpyObj<AuthService> {
  currentUser: BehaviorSubject<UserDetails | null>;
  currentUserValue: UserDetails | null;
  currentUserId: number | null;
}

describe('InventoryInfoComponent', () => {
  let component: InventoryInfoComponent;
  let fixture: ComponentFixture<InventoryInfoComponent>;
  let mockActivatedRoute: { snapshot: { paramMap: Map<string, string> }, paramMap: BehaviorSubject<Map<string, string>> };
  let mockRouter: jasmine.SpyObj<Router>;
  let mockInventoryService: jasmine.SpyObj<InventoryService>;
  let mockProductService: jasmine.SpyObj<ProductService>;
  let mockCategoryService: jasmine.SpyObj<CategoryService>;
  let mockAuthService: MockAuthService;
  let mockUserService: jasmine.SpyObj<UserService>;

  const mockInventory: Inventory = {
    inventoryId: 1,
    name: 'Main Warehouse',
    location: 'Central City',
  };

  const mockProductsInInventory: ProductsForInventories[] = [
    { id: 1, productId: 101, productName: 'Laptop', sku: 'LP001', quantityInInventory: 50, minStockQuantity: 10, description: 'Gaming Laptop', unitPrice: 1200, categoryName: 'Electronics', categoryId: 1 },
    { id: 2, productId: 102, productName: 'Mouse', sku: 'MS001', quantityInInventory: 5, minStockQuantity: 10, description: 'Wireless Mouse', unitPrice: 25, categoryName: 'Electronics', categoryId: 1 },
  ];

  const mockAvailableProducts: Product[] = [
    { productId: 201, productName: 'Keyboard', sku: 'KB001', description: 'Mechanical', unitPrice: 75, categoryId: 1, categoryName: 'Electronics', isDeleted: false },
    { productId: 202, productName: 'Monitor', sku: 'MN001', description: '4K Display', unitPrice: 300, categoryId: 1, categoryName: 'Electronics', isDeleted: false },
  ];

  const mockAssignedManagers: ManagerUser[] = [
    { userId: 123, username: 'manager1', email: 'm1@example.com', phone: '1', profilePictureUrl: '', roleName: 'Manager' },
  ];

  const mockAllUsers: UserDetails[] = [
    { userId: 123, username: 'manager1', email: 'm1@example.com', phone: '1', profilePictureUrl: '', roleId: 2, roleName: 'Manager', isDeleted: false },
    { userId: 456, username: 'user1', email: 'u1@example.com', phone: '2', profilePictureUrl: '', roleId: 3, roleName: 'User', isDeleted: false },
  ];

  beforeEach(async () => {
    mockActivatedRoute = {
      snapshot: {
        paramMap: new Map([['id', '1']])
      },
      paramMap: new BehaviorSubject(new Map([['id', '1']]))
    };
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockInventoryService = jasmine.createSpyObj('InventoryService', [
      'getInventoryById', 'getProductsInInventory', 'createInventoryProduct',
      'increaseInventoryProductQuantity', 'decreaseInventoryProductQuantity',
      'setInventoryProductQuantity', 'updateInventoryProductMinStock',
      'deleteInventoryProduct', 'softDeleteInventory', 'getManagersByInventoryId',
      'assignInventoryToManager', 'removeInventoryFromManager', 'updateInventory'
    ]);
    mockProductService = jasmine.createSpyObj('ProductService', ['getAllProducts']);
    mockCategoryService = jasmine.createSpyObj('CategoryService', ['getAllCategories']);
    mockAuthService = jasmine.createSpyObj<AuthService>('AuthService', ['isAdmin', 'isManager', 'currentUserValue', 'currentUserId']) as MockAuthService;
    mockUserService = jasmine.createSpyObj('UserService', ['getAllUsers']);

    mockInventoryService.getInventoryById.and.returnValue(of(mockInventory));
    mockInventoryService.getProductsInInventory.and.returnValue(of({ data: mockProductsInInventory, pagination: { totalRecords: 2, page: 1, pageSize: 10, totalPages: 1 } }));
    mockInventoryService.getManagersByInventoryId.and.returnValue(of(mockAssignedManagers));
    mockProductService.getAllProducts.and.returnValue(of({ data: mockAvailableProducts, pagination: { totalRecords: 2, page: 1, pageSize: 10, totalPages: 1 } }));
    mockCategoryService.getAllCategories.and.returnValue(of([]));
    mockAuthService.isAdmin.and.returnValue(false);
    mockAuthService.isManager.and.returnValue(false);
    mockAuthService.currentUser = new BehaviorSubject<UserDetails | null>(null);
    Object.defineProperty(mockAuthService, 'currentUserValue', { configurable: true, get: () => mockAuthService.currentUser.value });
    Object.defineProperty(mockAuthService, 'currentUserId', { configurable: true, get: () => 123 });
    mockUserService.getAllUsers.and.returnValue(of({ data: mockAllUsers, pagination: { totalRecords: 2, page: 1, pageSize: 10, totalPages: 1 } }));

    await TestBed.configureTestingModule({
      imports: [
        InventoryInfoComponent,
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        PageNumberComponent,
        NgSelectModule,
      ],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        { provide: InventoryService, useValue: mockInventoryService },
        { provide: ProductService, useValue: mockProductService },
        { provide: CategoryService, useValue: mockCategoryService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: UserService, useValue: mockUserService },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(InventoryInfoComponent);
    component = fixture.componentInstance;
  });

  it('should create and load inventory details and products on ngOnInit', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);
    flush();
    expect(component).toBeTruthy();
    expect(component.productsInInventory).toEqual(mockProductsInInventory);
    expect(component.loading).toBeFalse();
    expect(mockInventoryService.getInventoryById).toHaveBeenCalledWith(1);
    expect(mockInventoryService.getProductsInInventory).toHaveBeenCalledWith(1, 1, 10, '', 'productName_asc');
  }));

  it('should display "No products found" message when inventory has no products', fakeAsync(() => {
    mockInventoryService.getProductsInInventory.and.returnValue(of({ data: [], pagination: { totalRecords: 0, page: 1, pageSize: 10, totalPages: 0 } }));
    fixture.detectChanges();
    tick(300);
    flush();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.alert-info')?.textContent).toContain('No products found in this inventory');
    expect(component.productsInInventory.length).toBe(0);
  }));

  it('should toggle add product form visibility', () => {
    expect(component.showAddProductForm).toBeFalse();
    component.openAddProductForm();
    expect(component.showAddProductForm).toBeTrue();
    component.cancelAddProduct();
    expect(component.showAddProductForm).toBeFalse();
  });

  it('should add an existing product to inventory', fakeAsync(() => {
    spyOn(window, 'alert');
    mockInventoryService.createInventoryProduct.and.returnValue(of({} as any));
    spyOn(component, 'fetchProductsInInventory');
    spyOn(component, 'fetchAllAvailableProducts');

    component.inventoryId = 1;
    component.selectedProductToAddId = 201;
    component.newProductQuantity = 10;
    component.newProductMinStock = 2;

    component.addProductToInventory();
    tick();
    flush();

    expect(mockInventoryService.createInventoryProduct).toHaveBeenCalledWith({
      inventoryId: 1,
      productId: 201,
      quantity: 10,
      minStockQuantity: 2
    });
    expect(window.alert).toHaveBeenCalledWith('Product added to inventory successfully!');
    expect(component.showAddProductForm).toBeFalse();
    expect(component.fetchProductsInInventory).toHaveBeenCalled();
    expect(component.fetchAllAvailableProducts).toHaveBeenCalled();
  }));
});
