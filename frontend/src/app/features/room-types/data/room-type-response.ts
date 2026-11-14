import { RoomCategory } from './room-category';

export interface RoomTypeResponse {
  id: string;
  hotelId: string;
  name: string;
  category: RoomCategory;
  maxOccupancy: number;
}
