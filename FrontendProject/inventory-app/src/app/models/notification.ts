export interface LowStockNotificationDto {
  productId: number;
  productName: string;
  sku: string | null;
  currentQuantity: number | null;
  minStockQuantity: number | null;
  inventoryId: number;
  inventoryName: string;
  message: string;
  timestamp: string; // ISO 8601 string
}
