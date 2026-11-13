using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Domain.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Backoffice.Application.Common;

public interface IBackofficeDbContext
{
    DbSet<Hotel> Hotels { get; }

    DbSet<RoomType> RoomTypes { get; }

    DbSet<InventoryDay> InventoryDays { get; }

    DbSet<Booking> Bookings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
