import { RoomTypeInventoryResponse } from './room-type-inventory-response';

export interface InventoryCalendarResponse {
  hotelId: string;
  hotelName: string;
  currency: string;
  from: string;
  to: string;
  roomTypes: RoomTypeInventoryResponse[];
}
