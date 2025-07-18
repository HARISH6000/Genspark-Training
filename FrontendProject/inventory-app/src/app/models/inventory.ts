export interface Inventory {
  inventoryId: number;
  name: string;
  location: string;
  isDeleted?: boolean;
}

export interface CreateInventoryRequest {
  name: string;
  location: string;
}

export interface UpdateInventoryRequest {
  inventoryId: number;
  name: string;
  location: string;
}

export interface InventoryProduct {
  id: number;
  inventoryId: number;
  inventoryName: string;
  inventoryLocation: string;
  productId: number;
  productName: string;
  productSKU: string;
  quantity: number;
  minStockQuantity: number;
}

export interface CreateInventoryProductRequest {
  inventoryId: number;
  productId: number;
  quantity: number;
  minStockQuantity: number;
}

export interface QuantityChangeRequest {
  inventoryId: number;
  productId: number;
  quantityChange: number;
}

export interface SetQuantityRequest {
  inventoryId: number;
  productId: number;
  newQuantity: number;
}

export interface UpdateMinStockRequest {
  inventoryId: number;
  productId: number;
  newMinStockQuantity: number;
}

export interface Product {
  productId: number;
  sku: string;
  productName: string;
  description: string;
  unitPrice: number;
  categoryId: number;
  categoryName: string;
  isDeleted?: boolean;
  quantityInInventory?: number;
}


export interface Category {
  categoryId: number;
  categoryName: string;
  description: string;
  productCount?: number;
}


export interface InventoryManager {
  id: number;
  inventoryId: number;
  inventoryName: string;
  inventoryLocation: string;
  managerId: number;
  managerUsername: string;
  managerEmail: string;
}

export interface InventoryManagerAssignmentRequest {
  inventoryId: number;
  managerId: number;
}

export interface ManagerUser {
  userId: number;
  username: string;
  email: string;
  phone: string;
  profilePictureUrl: string;
  roleName: string;
}

export interface InventoriesForProduct {
  inventoryId: number;
  inventoryName: string;
  inventoryLocation: string;
  quantityInInventory: number;
  minStockQuantity: number;
}

export interface ProductsForInventories {
  id:number;
  productId: number;
  productName: string;
  sku: string;
  description: string;
  unitPrice: number;
  categoryId: number;
  categoryName: string;
  quantityInInventory: number;
  minStockQuantity: number;
}
