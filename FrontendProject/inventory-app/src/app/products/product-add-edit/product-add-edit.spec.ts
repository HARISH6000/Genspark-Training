import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductAddEditComponent } from './product-add-edit';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { of, throwError } from 'rxjs';
import { Product, AddProductRequest, UpdateProductRequest } from '../../models/product';
import { Category } from '../../models/inventory'; // Assuming Category is from inventory models
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

describe('ProductAddEditComponent', () => {
  let component: ProductAddEditComponent;
  let fixture: ComponentFixture<ProductAddEditComponent>;
  let mockActivatedRoute: any;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockProductService: jasmine.SpyObj<ProductService>;
  let mockCategoryService: jasmine.SpyObj<CategoryService>;

  const mockCategories: Category[] = [
    { categoryId: 1, categoryName: 'Electronics', description: 'Electronic items' },
    { categoryId: 2, categoryName: 'Books', description: 'Books and literature' },
  ];

  const mockProduct: Product = {
    productId: 1,
    sku: 'SKU001',
    productName: 'Test Product',
    description: 'A test product description',
    unitPrice: 10.99,
    categoryId: 1,
    categoryName: 'Electronics',
    isDeleted: false,
  };

  beforeEach(async () => {
    mockActivatedRoute = {
      snapshot: {
        paramMap: new Map() // Default to no ID (add mode)
      }
    };
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockProductService = jasmine.createSpyObj('ProductService', ['getProductById', 'addProduct', 'updateProduct']);
    mockCategoryService = jasmine.createSpyObj('CategoryService', ['getAllCategories']);

    // Default mock service responses
    mockCategoryService.getAllCategories.and.returnValue(of(mockCategories));
    mockProductService.getProductById.and.returnValue(of(mockProduct));
    mockProductService.addProduct.and.returnValue(of({ ...mockProduct, productId: 2 })); // Simulate new product added
    mockProductService.updateProduct.and.returnValue(of(mockProduct));

    await TestBed.configureTestingModule({
      imports: [
        ProductAddEditComponent, // Standalone component
        FormsModule, // For ngModel
        CommonModule // For *ngIf, *ngFor
      ],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        { provide: ProductService, useValue: mockProductService },
        { provide: CategoryService, useValue: mockCategoryService },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductAddEditComponent);
    component = fixture.componentInstance;
    // fixture.detectChanges(); // Call manually in tests for better control
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  describe('ngOnInit (Add Mode)', () => {
    it('should fetch categories and be in add mode when no product ID is provided', () => {
      // paramMap is already default to empty
      fixture.detectChanges(); // Trigger ngOnInit

      expect(component.isEditMode).toBeFalse();
      expect(component.productId).toBeNull();
      expect(mockCategoryService.getAllCategories).toHaveBeenCalled();
      expect(component.categories).toEqual(mockCategories);
      expect(mockProductService.getProductById).not.toHaveBeenCalled();
      expect(component.loading).toBeFalse(); // Should not be loading product details
    });
  });

  describe('ngOnInit (Edit Mode)', () => {
    beforeEach(() => {
      // Set up ActivatedRoute for edit mode
      mockActivatedRoute.snapshot.paramMap = new Map([['id', '1']]);
    });

    it('should fetch categories and product details and be in edit mode when product ID is provided', () => {
      fixture.detectChanges(); // Trigger ngOnInit

      expect(component.isEditMode).toBeTrue();
      expect(component.productId).toBe(1);
      expect(mockCategoryService.getAllCategories).toHaveBeenCalled();
      expect(component.categories).toEqual(mockCategories);
      expect(mockProductService.getProductById).toHaveBeenCalledWith(1);
      expect(component.product).toEqual({
        productId: mockProduct.productId,
        sku: mockProduct.sku,
        productName: mockProduct.productName,
        description: mockProduct.description,
        unitPrice: mockProduct.unitPrice,
        categoryId: mockProduct.categoryId
      } as UpdateProductRequest);
      expect(component.loading).toBeFalse();
    });

    it('should handle error when fetching product details in edit mode', () => {
      mockProductService.getProductById.and.returnValue(throwError(() => new Error('Product fetch failed')));
      fixture.detectChanges(); // Trigger ngOnInit

      expect(component.isEditMode).toBeTrue();
      expect(component.productId).toBe(1);
      expect(mockProductService.getProductById).toHaveBeenCalledWith(1);
      // FIX: Expect the exact error message from the thrown error
      expect(component.errorMessage).toBe('Product fetch failed');
      expect(component.loading).toBeFalse();
      // FIX: Expect router.navigate to be called, as the component navigates on null product (after error caught)
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/products']);
    });

    it('should redirect to /products if product not found in edit mode', () => {
      // Simulate an error from getProductById, which will be caught by the component
      mockProductService.getProductById.and.returnValue(throwError(() => new Error('Product not found (404)')));
      fixture.detectChanges(); // Trigger ngOnInit

      expect(component.isEditMode).toBeTrue();
      expect(component.productId).toBe(1);
      expect(mockProductService.getProductById).toHaveBeenCalledWith(1);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/products']);
      expect(component.loading).toBeFalse();
      // FIX: Expect the exact error message from the thrown error
      expect(component.errorMessage).toBe('Product not found (404)');
    });
  });

  describe('onSubmit (Add Mode)', () => {
    beforeEach(() => {
      // Ensure component is in add mode
      mockActivatedRoute.snapshot.paramMap = new Map();
      fixture.detectChanges(); // Trigger ngOnInit for add mode setup
      component.product = {
        sku: 'NEW_SKU',
        productName: 'New Product',
        description: 'New Description',
        unitPrice: 50.00,
        categoryId: 1
      };
    });

    it('should call addProduct and navigate on success', (done) => { // FIX: Use done for async operations
      mockProductService.addProduct.and.returnValue(of({ ...mockProduct, productId: 2, sku: 'NEW_SKU' }));
      component.onSubmit();
      expect(component.loading).toBeFalse();
      expect(mockProductService.addProduct).toHaveBeenCalledWith(component.product as AddProductRequest);
      
      
      setTimeout(() => {
        expect(component.successMessage).toBe('Product added successfully!');
        expect(mockRouter.navigate).toHaveBeenCalledWith(['/products']);
        done();
      }, 0);
    });

    it('should set errorMessage on addProduct failure', (done) => { // FIX: Use done for async operations
      mockProductService.addProduct.and.returnValue(throwError(() => new Error('Failed to add product.')));
      component.onSubmit();
      
      // FIX: Use setTimeout to allow the observable to complete and finalize to run
      setTimeout(() => {
        expect(component.errorMessage).toBe('Failed to add product.');
        expect(component.loading).toBeFalse();
        expect(mockRouter.navigate).not.toHaveBeenCalled();
        done();
      }, 0); // Use 0ms to allow microtask queue to clear
    });
  });

  describe('onSubmit (Edit Mode)', () => {
    beforeEach(() => {
      // Ensure component is in edit mode
      mockActivatedRoute.snapshot.paramMap = new Map([['id', '1']]);
      fixture.detectChanges(); // Trigger ngOnInit for edit mode setup
      component.product = { ...mockProduct, productName: 'Updated Product Name' }; // Simulate user editing
    });

    it('should call updateProduct and navigate on success', (done) => { 
      mockProductService.updateProduct.and.returnValue(of(mockProduct));
      component.onSubmit();
      expect(component.loading).toBeFalse();
      expect(mockProductService.updateProduct).toHaveBeenCalledWith(component.product as UpdateProductRequest);
      
      // FIX: Use setTimeout to allow the observable to complete and finalize to run
      setTimeout(() => {
        expect(component.successMessage).toBe('Product updated successfully!');
        expect(mockRouter.navigate).toHaveBeenCalledWith(['/products']);
        done();
      }, 0); // Use 0ms to allow microtask queue to clear
    });

    it('should set errorMessage on updateProduct failure', (done) => { // FIX: Use done for async operations
      mockProductService.updateProduct.and.returnValue(throwError(() => new Error('Failed to update product.')));
      component.onSubmit();
      
      // FIX: Use setTimeout to allow the observable to complete and finalize to run
      setTimeout(() => {
        expect(component.errorMessage).toBe('Failed to update product.');
        expect(component.loading).toBeFalse(); // FIX: Expect loading to be false after error
        expect(mockRouter.navigate).not.toHaveBeenCalled();
        done();
      }, 0); // Use 0ms to allow microtask queue to clear
    });
  });

  it('onCancel should navigate to /products', () => {
    component.onCancel();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/products']);
  });

  it('should fetch categories on fetchCategories', () => {
    // Reset calls to ensure only this test's call is counted
    mockCategoryService.getAllCategories.calls.reset();
    component.fetchCategories();
    expect(mockCategoryService.getAllCategories).toHaveBeenCalled();
    expect(component.categories).toEqual(mockCategories);
  });

  it('should set errorMessage on fetchCategories failure', (done) => { // FIX: Use done for async operations
    mockCategoryService.getAllCategories.and.returnValue(throwError(() => new Error('Category fetch failed')));
    component.fetchCategories();
    
    // FIX: Use setTimeout to allow the observable to complete
    setTimeout(() => {
      expect(component.errorMessage).toBe('Category fetch failed');
      expect(component.categories).toEqual([]); // Should be empty on error
      done();
    }, 0); // Use 0ms to allow microtask queue to clear
  });
});
