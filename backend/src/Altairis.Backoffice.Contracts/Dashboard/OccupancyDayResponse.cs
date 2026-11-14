namespace Altairis.Backoffice.Contracts.Dashboard;

public sealed record OccupancyDayResponse(DateOnly Date, int BookedRooms, int TotalRooms);
