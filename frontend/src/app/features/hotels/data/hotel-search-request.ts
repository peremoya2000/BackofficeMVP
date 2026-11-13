import { PagedRequest } from '../../../core/http/paged-request';

export interface HotelSearchRequest extends PagedRequest {
  search?: string;
  countryCode?: string;
  stars?: number;
}
