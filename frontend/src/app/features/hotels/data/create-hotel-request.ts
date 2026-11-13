export interface CreateHotelRequest {
  name: string;
  stars: number;
  address: string;
  city: string;
  countryCode: string;
  contactEmail: string | null;
  contactPhone: string | null;
}
