import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductInfoComponent } from './product-info';
import { ActivatedRoute, Router } from '@angular/router'; // FIX: Removed provideRouter import
import { ProductService } from '../../services/product.service';
import { InventoryService } from '../../services/inventory.service';
import { AuthService, UserDetails } from '../../services/auth.service';
import { of, throwError, BehaviorSubject, Observable } from 'rxjs';
import { Product } from '../../models/product';
import { InventoriesForProduct } from '../../models/inventory';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

interface MockAuthService extends jasmine.SpyObj<AuthService> {
  currentUser: BehaviorSubject<UserDetails | null>;
  currentUserValue: UserDetails | null;
}

describe('ProductInfoComponent', () => {
  let component: ProductInfoComponent;
  let fixture: ComponentFixture<ProductInfoComponent>;
  let mockActivatedRoute: any;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockProductService: jasmine.SpyObj<ProductService>;
  let mockInventoryService: jasmine.SpyObj<InventoryService>;
  let mockAuthService: MockAuthService;

  const mockProduct: Product = {
    productId: 1,
    sku: 'PROD001',
    productName: 'Test Product',
    description: 'A test product',
    unitPrice: 100,
    categoryId: 1,
    categoryName: 'Electronics',
    isDeleted: false,
  };

  const mockInventories: InventoriesForProduct[] = [
    { inventoryId: 1, inventoryName: 'Warehouse A', inventoryLocation: 'Loc A', quantityInInventory: 50, minStockQuantity: 10 },
    { inventoryId: 2, inventoryName: 'Warehouse B', inventoryLocation: 'Loc B', quantityInInventory: 5, minStockQuantity: 10 }, // Low stock
  ];

  const mockAdminUser: UserDetails = {
    userId: 1, username: 'admin', email: 'a@a.com', phone: '1', profilePictureUrl: '', roleId: 1, roleName: 'Admin', isDeleted: false
  };
  const mockManagerUser: UserDetails = {
    userId: 2, username: 'manager', email: 'm@m.com', phone: '2', profilePictureUrl: '', roleId: 2, roleName: 'Manager', isDeleted: false
  };


  beforeEach(async () => {
    mockActivatedRoute = {
      paramMap: of(new Map([['id', '1']]))
    };
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockProductService = jasmine.createSpyObj('ProductService', ['getProductById', 'softDeleteProduct', 'hardDeleteProduct']);
    mockInventoryService = jasmine.createSpyObj('InventoryService', ['getInventoriesForProduct']);
    mockAuthService = jasmine.createSpyObj<AuthService>('AuthService', ['isAdmin', 'isManager', 'currentUserValue']) as MockAuthService;

    mockProductService.getProductById.and.returnValue(of(mockProduct));
    mockInventoryService.getInventoriesForProduct.and.returnValue(of({ data: mockInventories, pagination: { totalRecords: 2, page: 1, pageSize: 100, totalPages: 1 } }));
    mockAuthService.isAdmin.and.returnValue(false);
    mockAuthService.isManager.and.returnValue(false);
    mockAuthService.currentUser = new BehaviorSubject<UserDetails | null>(null);
    Object.defineProperty(mockAuthService, 'currentUserValue', {
      configurable: true,
      get: () => mockAuthService.currentUser.value
    });


    await TestBed.configureTestingModule({
      imports: [
        ProductInfoComponent,
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        CurrencyPipe
      ],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        { provide: ProductService, useValue: mockProductService },
        { provide: InventoryService, useValue: mockInventoryService },
        { provide: AuthService, useValue: mockAuthService },
        // FIX: Removed provideRouter([]) as it conflicts with custom Router mock
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load product details and inventories on ngOnInit', () => {
    expect(component.loadingProduct).toBeFalse();
    expect(component.product).toEqual(mockProduct);
    expect(mockProductService.getProductById).toHaveBeenCalledWith(1);

    expect(component.loadingInventories).toBeFalse();
    expect(component.inventories).toEqual(mockInventories);
    expect(component.filteredInventories).toEqual(mockInventories);
    expect(mockInventoryService.getInventoriesForProduct).toHaveBeenCalledWith(1, 1, 100, '', '');
  });

  it('should display product not found message if no product ID', () => {
    mockActivatedRoute.paramMap = of(new Map());
    fixture = TestBed.createComponent(ProductInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.loadingProduct).toBeFalse();
    expect(component.productErrorMessage).toBe('Product ID not provided.');
  });

  it('should handle error when fetching product details', () => {
    mockProductService.getProductById.and.returnValue(throwError(() => new Error('Product fetch failed')));
    component.fetchProductDetails(1);
    expect(component.loadingProduct).toBeFalse();
    expect(component.productErrorMessage).toBe('Product fetch failed');
    expect(component.product).toBeNull();
  });

  it('should handle error when fetching inventories', () => {
    mockInventoryService.getInventoriesForProduct.and.returnValue(throwError(() => new Error('Inventory fetch failed')));
    component.fetchInventoriesForProduct(1);
    expect(component.loadingInventories).toBeFalse();
    expect(component.inventoryErrorMessage).toBe('Inventory fetch failed');
    expect(component.inventories).toEqual([]);
    expect(component.filteredInventories).toEqual([]);
  });

  it('should navigate to edit product page', () => {
    component.onEditProduct(1);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/edit-product', 1]);
  });

  it('should soft delete product and refetch details', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    mockProductService.softDeleteProduct.and.returnValue(of({ ...mockProduct, isDeleted: true }));
    spyOn(component, 'fetchProductDetails');

    component.softDeleteProduct(1);

    expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to soft delete this product? It can be restored later.');
    expect(mockProductService.softDeleteProduct).toHaveBeenCalledWith(1);
    expect(component.fetchProductDetails).toHaveBeenCalledWith(1);
  });

  it('should hard delete product and navigate to products list', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    mockProductService.hardDeleteProduct.and.returnValue(of({ ...mockProduct, isDeleted: true }));

    component.hardDeleteProduct(1);

    expect(window.confirm).toHaveBeenCalledWith('WARNING: Are you sure you want to PERMANENTLY delete this product? This action cannot be undone.');
    expect(mockProductService.hardDeleteProduct).toHaveBeenCalledWith(1);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/products']);
  });

  it('isAdmin should return true if authService.isAdmin returns true', () => {
    mockAuthService.isAdmin.and.returnValue(true);
    expect(component.isAdmin()).toBeTrue();
  });

  it('isManagerOrAdmin should return true if authService.isManager returns true', () => {
    mockAuthService.isManager.and.returnValue(true);
    expect(component.isManagerOrAdmin()).toBeTrue();
  });

  it('isManagerOrAdmin should return true if authService.isAdmin returns true', () => {
    mockAuthService.isAdmin.and.returnValue(true);
    expect(component.isManagerOrAdmin()).toBeTrue();
  });

  it('should filter inventories by low stock', () => {
    component.inventories = mockInventories;
    component.showLowStockOnly = true;
    component.applyInventoryFilters();
    expect(component.filteredInventories).toEqual([mockInventories[1]]);
  });

  it('should trigger fetchInventoriesForProduct on inventory search control value changes', (done) => {
    spyOn(component, 'fetchInventoriesForProduct');
    component.inventorySearchControl.setValue('test');
    setTimeout(() => {
      expect(component.fetchInventoriesForProduct).toHaveBeenCalledWith(component.productId!);
      done();
    }, 350);
  });

  it('should add a sort criterion', () => {
    component.sortCriteria = [];
    component.addSortCriterion();
    expect(component.sortCriteria.length).toBe(1);
    expect(component.sortCriteria[0]).toEqual({ field: '', order: 'asc' });
  });

  it('should remove a sort criterion and trigger onSortCriterionChange', () => {
    component.sortCriteria = [{ field: 'name', order: 'asc' }, { field: 'quantity', order: 'desc' }];
    spyOn(component, 'onSortCriterionChange');
    component.removeSortCriterion(0);
    expect(component.sortCriteria.length).toBe(1);
    expect(component.sortCriteria[0]).toEqual({ field: 'quantity', order: 'desc' });
    expect(component.onSortCriterionChange).toHaveBeenCalled();
  });

  it('should call fetchInventoriesForProduct on onSortCriterionChange', () => {
    spyOn(component, 'fetchInventoriesForProduct');
    component.onSortCriterionChange();
    expect(component.fetchInventoriesForProduct).toHaveBeenCalledWith(component.productId!);
  });

  it('should navigate back to products list on goBack', () => {
    component.goBack();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/products']);
  });

  it('should unsubscribe from subscriptions on ngOnDestroy', () => {
    const routeSubSpy = spyOn((component as any)['routeSub'], 'unsubscribe');
    const inventorySearchSubSpy = spyOn((component as any)['inventorySearchSub'], 'unsubscribe');

    component.ngOnDestroy();

    expect(routeSubSpy).toHaveBeenCalledTimes(1);
    expect(inventorySearchSubSpy).toHaveBeenCalledTimes(1);
  });
});
