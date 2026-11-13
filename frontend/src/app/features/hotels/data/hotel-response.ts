export interface HotelResponse {
  id: string;
  name: string;
  stars: number;
  address: string;
  city: string;
  countryCode: string;
  currency: string;
  contactEmail: string | null;
  contactPhone: string | null;
  createdAtUtc: string;
}
