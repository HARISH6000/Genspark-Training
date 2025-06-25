import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RecipeCardComponent } from './recipe-card';
import { Recipe } from '../models/recipe';
import { CommonModule } from '@angular/common';

describe('RecipeCardComponent', () => {
  let component: RecipeCardComponent;
  let fixture: ComponentFixture<RecipeCardComponent>;
  let compiled: HTMLElement;

  const mockRecipe: Recipe = {
    id: 1,
    title: 'Delicious Spaghetti',
    description: 'A classic spaghetti recipe with rich tomato sauce and meatballs.',
    image: 'https://example.com/spaghetti.jpg',
    ingredients: ['Spaghetti', 'Ground Beef', 'Tomato Sauce', 'Onion', 'Garlic', 'Parmesan Cheese'],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecipeCardComponent, CommonModule],
    }).compileComponents();

    fixture = TestBed.createComponent(RecipeCardComponent);
    component = fixture.componentInstance;
    compiled = fixture.nativeElement;
    component.recipe = mockRecipe;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should display the recipe image correctly', () => {
    const imgElement: HTMLImageElement | null = compiled.querySelector('.recipe-card-image');
    expect(imgElement).toBeTruthy();
    expect(imgElement?.src).toContain(mockRecipe.image);
    expect(imgElement?.alt).toBe(mockRecipe.title);
  });

  it('should display the recipe title', () => {
    const titleElement: HTMLElement | null = compiled.querySelector('.recipe-card-title');
    expect(titleElement).toBeTruthy();
    expect(titleElement?.textContent).toContain(mockRecipe.title);
  });

  it('should display the recipe description', () => {
    const descriptionElement: HTMLElement | null = compiled.querySelector('.recipe-card-description');
    expect(descriptionElement).toBeTruthy();
    expect(descriptionElement?.textContent).toContain(mockRecipe.description);
  });

  it('should display all ingredients', () => {
    const ingredientItems: NodeListOf<HTMLElement> = compiled.querySelectorAll('.ingredient-item');
    expect(ingredientItems.length).toBe(mockRecipe.ingredients.length);

    mockRecipe.ingredients.forEach((ingredient, index) => {
      expect(ingredientItems[index].textContent).toContain(ingredient);
    });
  });

  it('should display the correct ingredient icon', () => {
    const ingredientIcons: NodeListOf<SVGElement> = compiled.querySelectorAll('.ingredient-icon');
    expect(ingredientIcons.length).toBe(mockRecipe.ingredients.length);
  });
});
