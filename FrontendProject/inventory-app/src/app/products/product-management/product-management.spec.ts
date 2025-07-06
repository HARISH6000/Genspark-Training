import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ProductManagementComponent } from './product-management';
import { Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { AuthService, UserDetails } from '../../services/auth.service';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { Product } from '../../models/product';
import { Category } from '../../models/inventory';
import { PaginationResponse } from '../../models/pagination-response';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageNumberComponent } from '../../shared/page-number/page-number';

interface MockAuthService extends jasmine.SpyObj<AuthService> {
  currentUser: BehaviorSubject<UserDetails | null>;
  currentUserValue: UserDetails | null;
}

describe('ProductManagementComponent', () => {
  let component: ProductManagementComponent;
  let fixture: ComponentFixture<ProductManagementComponent>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockProductService: jasmine.SpyObj<ProductService>;
  let mockCategoryService: jasmine.SpyObj<CategoryService>;
  let mockAuthService: MockAuthService;

  const mockCategories: Category[] = [
    { categoryId: 1, categoryName: 'Electronics', description: 'Electronic items' },
    { categoryId: 2, categoryName: 'Books', description: 'Books and literature' },
  ];

  const mockProducts: Product[] = [
    { productId: 1, sku: 'SKU001', productName: 'Laptop', description: 'Desc 1', unitPrice: 1200, categoryId: 1, categoryName: 'Electronics', isDeleted: false },
    { productId: 2, sku: 'SKU002', productName: 'Mouse', description: 'Desc 2', unitPrice: 25, categoryId: 1, categoryName: 'Electronics', isDeleted: false },
    { productId: 3, sku: 'SKU003', productName: 'Keyboard', description: 'Desc 3', unitPrice: 75, categoryId: 1, categoryName: 'Electronics', isDeleted: true },
  ];

  const mockPaginationResponse: PaginationResponse<Product> = {
    data: mockProducts,
    pagination: {
      totalRecords: 3,
      page: 1,
      pageSize: 10,
      totalPages: 1,
    },
  };

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockProductService = jasmine.createSpyObj('ProductService', ['getAllProducts', 'softDeleteProduct', 'hardDeleteProduct']);
    mockCategoryService = jasmine.createSpyObj('CategoryService', ['getAllCategories']);
    mockAuthService = jasmine.createSpyObj<AuthService>('AuthService', ['isAdmin', 'isManager', 'currentUserValue']) as MockAuthService;

    mockCategoryService.getAllCategories.and.returnValue(of(mockCategories));
    mockProductService.getAllProducts.and.returnValue(of(mockPaginationResponse));
    mockAuthService.isAdmin.and.returnValue(false);
    mockAuthService.isManager.and.returnValue(false);
    mockAuthService.currentUser = new BehaviorSubject<UserDetails | null>(null);
    Object.defineProperty(mockAuthService, 'currentUserValue', {
      configurable: true,
      get: () => mockAuthService.currentUser.value
    });

    await TestBed.configureTestingModule({
      imports: [
        ProductManagementComponent,
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        PageNumberComponent,
      ],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: ProductService, useValue: mockProductService },
        { provide: CategoryService, useValue: mockCategoryService },
        { provide: AuthService, useValue: mockAuthService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductManagementComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load categories and products on ngOnInit', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);
    expect(mockCategoryService.getAllCategories).toHaveBeenCalled();
    expect(component.categories).toEqual(mockCategories);
    expect(mockProductService.getAllProducts).toHaveBeenCalled();
    expect(component.products).toEqual(mockProducts);
    expect(component.filteredProducts.length).toBe(2);
    expect(component.filteredProducts).toEqual(mockProducts.filter(p => !p.isDeleted));
    expect(component.loading).toBeFalse();
  }));

  it('should filter products by category', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);
    component.selectedCategory = 1;
    component.onCategoryChange();
    tick(300);
    mockProductService.getAllProducts.and.returnValue(of({
      data: [mockProducts[0], mockProducts[1]],
      pagination: { totalRecords: 2, page: 1, pageSize: 10, totalPages: 1 }
    }));
    component.fetchProducts(null);
    tick(300);
    expect(component.filteredProducts.length).toBe(2);
    expect(component.filteredProducts[0].productName).toBe('Laptop');
    expect(component.filteredProducts[1].productName).toBe('Mouse');
  }));

  it('should show deleted products when showDeleted is true', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);
    component.showDeleted = true;
    component.onShowDeletedChange();
    tick(300);
    expect(mockProductService.getAllProducts).toHaveBeenCalledWith(1, 10, '', '', true);
    mockProductService.getAllProducts.and.returnValue(of(mockPaginationResponse));
    component.fetchProducts(null);
    tick(300);
    expect(component.filteredProducts.length).toBe(3);
  }));

  it('should navigate to add product page', () => {
    component.addProduct();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/add-product']);
  });

  it('should soft delete a product and refresh the list', fakeAsync(() => {
    spyOn(window, 'confirm').and.returnValue(true);
    mockProductService.softDeleteProduct.and.returnValue(of({ ...mockProducts[0], isDeleted: true }));
    spyOn(component, 'fetchProducts');
    component.softDeleteProduct(1);
    tick();
    expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to soft delete this product? It can be restored later.');
    expect(mockProductService.softDeleteProduct).toHaveBeenCalledWith(1);
    expect(component.fetchProducts).toHaveBeenCalled();
  }));

  it('should hard delete a product and refresh the list', fakeAsync(() => {
    spyOn(window, 'confirm').and.returnValue(true);
    mockProductService.hardDeleteProduct.and.returnValue(of(mockProducts[2]));
    spyOn(component, 'fetchProducts');
    component.hardDeleteProduct(3);
    tick();
    expect(window.confirm).toHaveBeenCalledWith('WARNING: Are you sure you want to PERMANENTLY delete this product? This action cannot be undone.');
    expect(mockProductService.hardDeleteProduct).toHaveBeenCalledWith(3);
    expect(component.fetchProducts).toHaveBeenCalled();
  }));

  it('isAdmin should return true if authService.isAdmin returns true', () => {
    mockAuthService.isAdmin.and.returnValue(true);
    expect(component.isAdmin()).toBeTrue();
  });

  it('isManagerOrAdmin should return true if authService.isManager returns true', () => {
    mockAuthService.isManager.and.returnValue(true);
    expect(component.isManagerOrAdmin()).toBeTrue();
  });

  it('should navigate to product details page', () => {
    component.viewProductDetails(1);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/product-info', 1]);
  });

  it('should navigate to edit product page', () => {
    component.editProduct(1);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/edit-product', 1]);
  });

  it('should change page on onPageChange', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);
    spyOn(component, 'fetchProducts');
    component.totalPages = 5;
    component.onPageChange(2);
    expect(component.pageNumber).toBe(2);
    expect(component.fetchProducts).toHaveBeenCalled();
  }));

  it('should change page size on onPageSizeChange', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);
    spyOn(component, 'fetchProducts');
    component.pageSize = 5;
    component.onPageSizeChange();
    expect(component.pageNumber).toBe(1);
    expect(component.fetchProducts).toHaveBeenCalled();
  }));

  it('should add a sort criterion', () => {
    component.sortCriteria = [];
    component.addSortCriterion();
    expect(component.sortCriteria.length).toBe(1);
    expect(component.sortCriteria[0]).toEqual({ field: '', order: 'asc' });
  });

  it('should remove a sort criterion and trigger onSortChange', () => {
    component.sortCriteria = [{ field: 'productName', order: 'asc' }];
    spyOn(component, 'onSortChange');
    component.removeSortCriterion(0);
    expect(component.sortCriteria.length).toBe(0);
    expect(component.onSortChange).toHaveBeenCalled();
  });

  it('should call fetchProducts on onSortChange', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);
    spyOn(component, 'fetchProducts');
    component.onSortChange();
    expect(component.fetchProducts).toHaveBeenCalled();
  }));
});
