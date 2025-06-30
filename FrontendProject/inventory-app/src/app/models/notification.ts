export interface LowStockNotificationDto {
  productId: number;
  productName: string;
  sku: string;
  currentQuantity: number;
  minStockQuantity: number;
  inventoryId: number;
  inventoryName: string;
  message: string;
  timestamp: string; // ISO 8601 string
}