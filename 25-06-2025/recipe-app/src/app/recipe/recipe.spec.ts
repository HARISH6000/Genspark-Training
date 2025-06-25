import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { RecipeComponent } from './recipe';
import { RecipeService } from '../services/recipe';
import { Recipe } from '../models/recipe';
import { of, throwError } from 'rxjs';
import { CommonModule } from '@angular/common';
import { RecipeCardComponent } from '../recipe-card/recipe-card';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

describe('RecipeComponent', () => {
  let component: RecipeComponent;
  let fixture: ComponentFixture<RecipeComponent>;
  let mockRecipeService: jasmine.SpyObj<RecipeService>;
  let compiled: HTMLElement;

  const mockRecipes: Recipe[] = [
    {
      id: 1,
      title: 'Pasta',
      description: 'Delicious pasta recipe',
      image: 'pasta.jpg',
      ingredients: ['pasta', 'tomato sauce'],
    },
    {
      id: 2,
      title: 'Salad',
      description: 'Fresh salad recipe',
      image: 'salad.jpg',
      ingredients: ['lettuce', 'cucumber'],
    },
  ];

  beforeEach(async () => {
    mockRecipeService = jasmine.createSpyObj('RecipeService', ['getRecipes']);
    await TestBed.configureTestingModule({
      imports: [RecipeComponent, CommonModule, RecipeCardComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RecipeService, useValue: mockRecipeService },
      ],
    })
      .overrideComponent(RecipeComponent, { set: { providers: [] } })
      .compileComponents();

    fixture = TestBed.createComponent(RecipeComponent);
    component = fixture.componentInstance;
    compiled = fixture.nativeElement;
  });

  afterEach(() => {
    const errorDiv = document.body.querySelector('.fixed.inset-0.bg-red-500');
    if (errorDiv) {
      errorDiv.remove();
    }
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with loading set to true and an empty recipes array', () => {
    expect(component.loading).toBeTrue();
    expect(component.recipes).toEqual([]);
  });

  it('should call getRecipes on ngOnInit and populate recipes on success', fakeAsync(() => {
    mockRecipeService.getRecipes.and.returnValue(of({ recipes: mockRecipes }));
    fixture.detectChanges();
    expect(mockRecipeService.getRecipes).toHaveBeenCalledTimes(1);
    tick();
    expect(component.recipes).toEqual(mockRecipes);
    expect(component.loading).toBeFalse();
    const recipeCards = compiled.querySelectorAll('app-recipe-card');
    expect(recipeCards.length).toBe(mockRecipes.length);
    expect(compiled.textContent).toContain(mockRecipes[0].title);
    expect(compiled.textContent).toContain(mockRecipes[1].title);
  }));

  it('should hide loading indicator and show recipe grid when loading is false', fakeAsync(() => {
    mockRecipeService.getRecipes.and.returnValue(of({ recipes: mockRecipes }));
    fixture.detectChanges();
    tick();
    expect(component.loading).toBeFalse();
    const loadingIndicator = compiled.querySelector('.loading-indicator');
    expect(loadingIndicator).toBeNull();
    const recipeGrid = compiled.querySelector('.recipe-grid');
    expect(recipeGrid).toBeTruthy();
  }));

  it('should handle API error and display an error message', fakeAsync(() => {
    const errorMessage = 'Failed to fetch recipes!';
    const consoleSpy = spyOn(console, 'error');
    mockRecipeService.getRecipes.and.returnValue(throwError(() => new Error(errorMessage)));
    fixture.detectChanges();
    tick();
    expect(mockRecipeService.getRecipes).toHaveBeenCalledTimes(1);
    expect(component.loading).toBeFalse();
    expect(component.recipes).toEqual([]);
    const errorDiv = document.body.querySelector('.fixed.inset-0.bg-red-500');
    expect(errorDiv).toBeTruthy();
    expect(errorDiv?.querySelector('p')?.textContent).toContain('Failed to load recipes. Please try again later.');
    expect(consoleSpy).toHaveBeenCalledWith('Failed to load recipes:', jasmine.any(Error));
  }));

  it('should correctly create and append the error message div and remove it on close', () => {
    const testMessage = 'Test error message';
    (component as any).displayErrorMessage(testMessage);
    const errorDiv = document.body.querySelector('.fixed.inset-0.bg-red-500');
    expect(errorDiv).toBeTruthy();
    expect(errorDiv?.querySelector('p')?.textContent).toContain(testMessage);
    const closeButton = errorDiv?.querySelector('button');
    expect(closeButton).toBeTruthy();
    closeButton?.click();
    expect(document.body.querySelector('.fixed.inset-0.bg-red-500')).toBeNull();
  });
});
