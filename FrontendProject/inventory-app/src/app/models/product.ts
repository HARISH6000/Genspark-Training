export interface Product {
  productId: number;
  sku: string;
  productName: string;
  description: string;
  unitPrice: number;
  categoryId: number;
  categoryName: string; 
  isDeleted: boolean;
}


export interface AddProductRequest {
  sku: string;
  productName: string;
  description: string;
  unitPrice: number;
  categoryId: number;
}


export interface UpdateProductRequest {
  productId: number;
  sku: string;
  productName: string;
  description: string;
  unitPrice: number;
  categoryId: number;
}