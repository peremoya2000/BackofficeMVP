import { InventoryDayResponse } from './inventory-day-response';

export interface RoomTypeInventoryResponse {
  roomTypeId: string;
  name: string;
  days: InventoryDayResponse[];
}
