import { BookingChannel } from './booking-channel';

export interface CreateBookingRequest {
  roomTypeId: string;
  checkIn: string;
  checkOut: string;
  rooms: number;
  adults: number;
  children: number;
  leadGuestName: string;
  channel: BookingChannel;
  clientName: string;
  quotedTotal: number;
  clientReference: string | null;
  notes: string | null;
}
