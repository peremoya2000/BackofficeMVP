import { PagedRequest } from '../../../core/http/paged-request';
import { BookingStatus } from './booking-status';

export interface BookingSearchRequest extends PagedRequest {
  search?: string;
  status?: BookingStatus;
  checkIn?: string;
  checkOut?: string;
}
