import { RoomCategory } from './room-category';

export interface CreateRoomTypeRequest {
  name: string;
  category: RoomCategory;
  maxOccupancy: number;
}
