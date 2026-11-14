export type RoomCategory = 'Single' | 'Double' | 'Twin' | 'Triple' | 'Family' | 'Suite';

export const ROOM_CATEGORY_LABELS: Record<RoomCategory, string> = {
  Single: 'Individual',
  Double: 'Doble',
  Twin: 'Doble (dos camas)',
  Triple: 'Triple',
  Family: 'Familiar',
  Suite: 'Suite',
};
