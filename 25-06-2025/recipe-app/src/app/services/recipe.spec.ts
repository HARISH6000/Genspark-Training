import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { RecipeService } from './recipe';
import { Recipe } from '../models/recipe';
import { provideHttpClient } from '@angular/common/http'; 

describe('RecipeService', () => {
  let service: RecipeService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        RecipeService,
        provideHttpClient(), 
        provideHttpClientTesting() 
      ],
    });
    service = TestBed.inject(RecipeService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify(); 
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should retrieve recipes from the API', () => {
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

    service.getRecipes().subscribe((data) => {
      expect(data.recipes.length).toBe(2);
      expect(data.recipes).toEqual(mockRecipes);
    });

    const req = httpTestingController.expectOne('https://dummyjson.com/recipes');
    expect(req.request.method).toBe('GET');
    req.flush({ recipes: mockRecipes }); 
  });
});