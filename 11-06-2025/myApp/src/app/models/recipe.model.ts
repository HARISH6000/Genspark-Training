export interface RecipeModel {
    id: number;
    name: string;
    cuisine: string;
    prepTimeMinutes: number;
    cookTimeMinutes: number;
    servings: number;
    ingredients: string[];
    instructions: string[];
    image: string;
    rating: number;
    reviewCount: number;
    mealType: string[];
}
