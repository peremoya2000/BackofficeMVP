import { BookingStatus } from './booking-status';

export interface BookingListItemResponse {
  id: string;
  reference: string;
  status: BookingStatus;
  hotelId: string;
  hotelName: string;
  checkIn: string;
  checkOut: string;
  leadGuestName: string;
  clientName: string;
  createdAtUtc: string;
}
