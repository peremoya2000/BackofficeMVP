export interface UpdateInventoryRangeRequest {
  from: string;
  to: string;
  totalRooms: number | null;
  price: number | null;
  salesClosed: boolean | null;
}
