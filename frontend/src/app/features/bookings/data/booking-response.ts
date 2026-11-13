import { BookingChannel } from './booking-channel';
import { BookingStatus } from './booking-status';

export interface BookingResponse {
  id: string;
  reference: string;
  status: BookingStatus;
  hotelId: string;
  hotelName: string;
  roomTypeId: string;
  roomTypeName: string;
  checkIn: string;
  checkOut: string;
  rooms: number;
  adults: number;
  children: number;
  leadGuestName: string;
  channel: BookingChannel;
  clientName: string;
  clientReference: string | null;
  notes: string | null;
  totalPrice: number;
  currency: string;
  createdAtUtc: string;
  cancelledAtUtc: string | null;
}
