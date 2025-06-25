import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { Products } from './products';
import { ProductService } from '../services/product.service';
import { ProductModel } from '../models/product';
import { CartItem } from '../models/cartItem';
import { of } from 'rxjs';

describe('Products', () => {
  let component: Products;
  let fixture: ComponentFixture<Products>;
  let mockProductService: jasmine.SpyObj<ProductService>;

  beforeEach(async () => {
    mockProductService = jasmine.createSpyObj('ProductService', [
      'getAllProducts',
      'getProductSearchResult',
    ]);

    await TestBed.configureTestingModule({
      imports: [Products,FormsModule],
      providers: [{ provide: ProductService, useValue: mockProductService }],
    }).compileComponents();

    fixture = TestBed.createComponent(Products);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize products and handle search', () => {
    const mockProducts: ProductModel[] = [
      new ProductModel(1, 'Product 1', 100, 'thumb1.jpg', 'Description 1'),
      new ProductModel(2, 'Product 2', 200, 'thumb2.jpg', 'Description 2'),
    ];

    mockProductService.getProductSearchResult.and.returnValue(
      of({ products: mockProducts, total: 2 })
    );

    component.searchSubject.next('Test');
    component.ngOnInit();

    component.searchSubject.subscribe(() => {
      expect(mockProductService.getProductSearchResult).toHaveBeenCalledWith('Test', component.limit, component.skip);
    });

    fixture.detectChanges();

    expect(component.products).toEqual(mockProducts);
    expect(component.total).toBe(2);
    expect(component.loading).toBeFalse();
  });

  it('should add items to the cart', () => {
    const productId = 1;

    component.handleAddToCart(productId);

    expect(component.cartItems.length).toBe(1);
    expect(component.cartItems[0]).toEqual(new CartItem(productId, 1));
    expect(component.cartCount).toBe(1);

    component.handleAddToCart(productId);

    expect(component.cartItems[0].Count).toBe(2);
    expect(component.cartCount).toBe(2);
  });

  it('should load more products on scroll', () => {
    const mockProducts: ProductModel[] = [
      new ProductModel(3, 'Product 3', 300, 'thumb3.jpg', 'Description 3'),
      new ProductModel(4, 'Product 4', 400, 'thumb4.jpg', 'Description 4'),
    ];

    mockProductService.getProductSearchResult.and.returnValue(
      of({ products: mockProducts, total: 4 })
    );

    component.products = [
      new ProductModel(1, 'Product 1', 100, 'thumb1.jpg', 'Description 1'),
      new ProductModel(2, 'Product 2', 200, 'thumb2.jpg', 'Description 2'),
    ];

    component.total = 4;
    component.skip = 2;

    component.loadMore();

    expect(mockProductService.getProductSearchResult).toHaveBeenCalledWith(component.searchString, component.limit, component.skip);
    expect(component.products.length).toBe(2); // Existing products
  });

  it('should debounce search input', (done) => {
    const searchString = 'Product';
    component.searchString = searchString;

    const mockSearchResult = {
      products: [
        new ProductModel(1, 'Product 1', 100, 'thumb1.jpg', 'Description 1'),
      ],
      total: 1,
    };

    mockProductService.getProductSearchResult.and.returnValue(of(mockSearchResult));

    component.handleSearchProducts();

    setTimeout(() => {
      expect(mockProductService.getProductSearchResult).toHaveBeenCalledWith(searchString, component.limit, component.skip);
      expect(component.products.length).toBe(1);
      done();
    }, 5000); // Wait for debounce time
  });
});
