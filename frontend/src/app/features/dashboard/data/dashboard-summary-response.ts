export interface DashboardSummaryResponse {
  date: string;
  arrivals: number;
  departures: number;
  staysInProgress: number;
  newBookings: number;
  cancellations: number;
  bookedRooms: number;
  totalRooms: number;
}
