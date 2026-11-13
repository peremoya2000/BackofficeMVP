import { format } from 'date-fns';

export function toIsoDate(date: Date): string {
  return format(date, 'yyyy-MM-dd');
}
