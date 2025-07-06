import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CategoryManagementComponent } from './category-management';
import { CategoryService, Category } from '../../services/category.service';
import { AuthService } from '../../services/auth.service';
import { of, throwError } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

describe('CategoryManagementComponent', () => {
  let component: CategoryManagementComponent;
  let fixture: ComponentFixture<CategoryManagementComponent>;
  let mockCategoryService: jasmine.SpyObj<CategoryService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  const mockCategories: Category[] = [
    { categoryId: 1, categoryName: 'Electronics', description: 'Electronic items' },
    { categoryId: 2, categoryName: 'Books', description: 'Books and literature' },
  ];

  beforeEach(async () => {
    mockCategoryService = jasmine.createSpyObj('CategoryService', ['getAllCategories', 'createCategory', 'updateCategory', 'deleteCategory']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['isAdmin']);

    mockCategoryService.getAllCategories.and.returnValue(of(mockCategories));
    mockCategoryService.createCategory.and.returnValue(of({ categoryId: 3, categoryName: 'New Category', description: 'New Desc' }));
    mockCategoryService.updateCategory.and.returnValue(of({ categoryId: 1, categoryName: 'Updated Name', description: 'Updated Desc' }));
    mockCategoryService.deleteCategory.and.returnValue(of({ categoryId: 1, categoryName: 'Deleted', description: '' }));
    mockAuthService.isAdmin.and.returnValue(true);

    await TestBed.configureTestingModule({
      imports: [
        CategoryManagementComponent,
        CommonModule,
        FormsModule,
      ],
      providers: [
        { provide: CategoryService, useValue: mockCategoryService },
        { provide: AuthService, useValue: mockAuthService },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryManagementComponent);
    component = fixture.componentInstance;
  });

  it('should create and fetch categories on ngOnInit', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    expect(component).toBeTruthy();
    expect(mockCategoryService.getAllCategories).toHaveBeenCalled();
    expect(component.categories).toEqual(mockCategories);
    expect(component.loading).toBeFalse();
  }));

  it('should add a new category successfully', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    component.openAddForm();
    component.newCategory = { categoryName: 'New Category', description: 'Description for new category' };
    spyOn(component, 'fetchCategories');

    component.submitCategoryForm();
    tick();

    expect(mockCategoryService.createCategory).toHaveBeenCalledWith({ categoryName: 'New Category', description: 'Description for new category' });
    expect(component.showForm).toBeFalse();
    expect(component.fetchCategories).toHaveBeenCalled();
  }));
});
