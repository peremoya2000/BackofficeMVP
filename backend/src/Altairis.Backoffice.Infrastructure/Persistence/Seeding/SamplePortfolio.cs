using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Inventory;

namespace Altairis.Backoffice.Infrastructure.Persistence.Seeding;

internal sealed record SamplePortfolio(List<InventoryDay> Inventory, List<Booking> Bookings);
