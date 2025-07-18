import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService, Category, CreateCategoryRequest, UpdateCategoryRequest } from '../../services/category.service';
import { AuthService } from '../../services/auth.service'; // Assuming AuthService is used for role checks
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-category-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category-management.html',
  styleUrls: ['./category-management.css']
})
export class CategoryManagementComponent implements OnInit {
  categories: Category[] = [];
  loading: boolean = true;
  errorMessage: string | null = null;

  showForm: boolean = false;
  isEditMode: boolean = false;
  currentCategory: Category | null = null;
  newCategory: CreateCategoryRequest = { categoryName: '', description: '' };

  constructor(
    private categoryService: CategoryService,
    private authService: AuthService // Inject AuthService
  ) { }

  ngOnInit(): void {
    this.fetchCategories();
  }

  fetchCategories(): void {
    this.loading = true;
    this.errorMessage = null;
    this.categoryService.getAllCategories().pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to fetch categories.';
        this.loading = false;
        return of([]);
      })
    ).subscribe(categories => {
      console.log('Fetched categories:', categories); // Debugging line
      this.categories = categories;
      this.loading = false;
    });
  }

  openAddForm(): void {
    this.isEditMode = false;
    this.currentCategory = null;
    this.newCategory = { categoryName: '', description: '' };
    this.showForm = true;
  }

  openEditForm(category: Category): void {
    this.isEditMode = true;
    this.currentCategory = { ...category }; // Create a copy to avoid direct mutation
    this.newCategory = { categoryName: category.categoryName, description: category.description };
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.errorMessage = null; // Clear error when closing form
  }

  submitCategoryForm(): void {
    if (this.isEditMode && this.currentCategory) {
      this.updateCategory();
    } else {
      this.addCategory();
    }
  }

  addCategory(): void {
    this.errorMessage = null;
    this.categoryService.createCategory(this.newCategory).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to add category.';
        return of(null);
      })
    ).subscribe(response => {
      if (response) {
        this.fetchCategories(); // Refresh the list
        this.closeForm();
      }
    });
  }

  updateCategory(): void {
    if (!this.currentCategory) return;

    const updateRequest: UpdateCategoryRequest = {
      categoryId: this.currentCategory.categoryId,
      categoryName: this.newCategory.categoryName,
      description: this.newCategory.description
    };

    this.errorMessage = null;
    this.categoryService.updateCategory(updateRequest).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to update category.';
        return of(null);
      })
    ).subscribe(response => {
      if (response) {
        this.fetchCategories(); 
        this.closeForm();
      }
    });
  }

  deleteCategory(categoryId: number): void {
    if (confirm('Are you sure you want to delete this category? This action cannot be undone.')) {
      this.errorMessage = null;
      this.categoryService.deleteCategory(categoryId).pipe(
        catchError(error => {
          this.errorMessage = error.message || 'Failed to delete category.';
          return of(null);
        })
      ).subscribe(response => {
        if (response) {
          this.fetchCategories();
        }
      });
    }
  }

  
  isAdmin(): boolean {
    return this.authService.isAdmin();
  }
}