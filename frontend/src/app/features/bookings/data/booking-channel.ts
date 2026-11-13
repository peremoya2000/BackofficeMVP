export type BookingChannel = 'Agency' | 'TourOperator' | 'Corporate' | 'Direct';

export const BOOKING_CHANNEL_LABELS: Record<BookingChannel, string> = {
  Agency: 'Agencia de viajes',
  TourOperator: 'Turoperador',
  Corporate: 'Empresa',
  Direct: 'Venta directa',
};
