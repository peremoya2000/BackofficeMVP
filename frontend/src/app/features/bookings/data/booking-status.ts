export type BookingStatus = 'Confirmed' | 'Cancelled';

export const BOOKING_STATUS_LABELS: Record<BookingStatus, string> = {
  Confirmed: 'Confirmada',
  Cancelled: 'Cancelada',
};
