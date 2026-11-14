namespace Altairis.Backoffice.Contracts.Dashboard;

public sealed record DashboardSummaryResponse(
    DateOnly Date,
    int Arrivals,
    int Departures,
    int StaysInProgress,
    int NewBookings,
    int Cancellations,
    int BookedRooms,
    int TotalRooms);
