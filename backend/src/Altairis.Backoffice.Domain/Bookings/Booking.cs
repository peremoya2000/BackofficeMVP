using System.Diagnostics;
using System.Security.Cryptography;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Domain.Inventory;

namespace Altairis.Backoffice.Domain.Bookings;

public sealed class Booking
{
    public const int MAX_NIGHTS = 30;
    public const int REFERENCE_LENGTH = 10;
    public const int LEAD_GUEST_NAME_MAX_LENGTH = 150;
    public const int NOTES_MAX_LENGTH = 1000;
    public const int CLIENT_NAME_MAX_LENGTH = 200;
    public const int CLIENT_REFERENCE_MAX_LENGTH = 50;

    private const string REFERENCE_PREFIX = "ALT-";
    private const string REFERENCE_ALPHABET = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private Booking(
        Guid id,
        string reference,
        Guid hotelId,
        Guid roomTypeId,
        int rooms,
        int adults,
        int children,
        string leadGuestName,
        string? notes,
        BookingChannel channel,
        string clientName,
        string? clientReference,
        BookingStatus status,
        DateTime createdAtUtc,
        DateTime? cancelledAtUtc)
    {
        Id = id;
        Reference = reference;
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
        Rooms = rooms;
        Adults = adults;
        Children = children;
        LeadGuestName = leadGuestName;
        Notes = notes;
        Channel = channel;
        ClientName = clientName;
        ClientReference = clientReference;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        CancelledAtUtc = cancelledAtUtc;
    }

    public Guid Id { get; private set; }

    public string Reference { get; private set; }

    public Guid HotelId { get; private set; }

    public Guid RoomTypeId { get; private set; }

    public StayPeriod Stay { get; private set; }

    public int Rooms { get; private set; }

    public int Adults { get; private set; }

    public int Children { get; private set; }

    public string LeadGuestName { get; private set; }

    public string? Notes { get; private set; }

    public BookingChannel Channel { get; private set; }

    public string ClientName { get; private set; }

    public string? ClientReference { get; private set; }

    public Money TotalPrice { get; private set; }

    public BookingStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public static Result<Money> Quote(
        Guid roomTypeId,
        StayPeriod stay,
        int rooms,
        IReadOnlyCollection<InventoryDay> inventory,
        string currency,
        DateOnly today)
    {
        if (rooms < 1) return BookingErrors.RoomsInvalid;
        if (stay.Nights > MAX_NIGHTS) return BookingErrors.TooManyNights;
        if (stay.CheckIn < today) return BookingErrors.CheckInInPast;

        Dictionary<DateOnly, InventoryDay> inventoryByNight = InventoryByNight(roomTypeId, inventory);
        Money total = Money.Zero(currency);

        foreach (DateOnly night in stay.EachNight())
        {
            if (!inventoryByNight.TryGetValue(night, out InventoryDay? day)) return BookingErrors.NoInventory(night);

            Result check = day.CanReserve(rooms);
            if (check.IsFailure) return check.Error;

            total += new Money(day.Price, currency) * rooms;
        }

        return total;
    }

    public static Result<Booking> Create(
        RoomType roomType,
        StayPeriod stay,
        int rooms,
        int adults,
        int children,
        string leadGuestName,
        string? notes,
        BookingChannel channel,
        string clientName,
        string? clientReference,
        IReadOnlyCollection<InventoryDay> inventory,
        string currency,
        Money quotedTotal,
        DateOnly today,
        DateTime createdAtUtc)
    {
        Debug.Assert(createdAtUtc.Kind == DateTimeKind.Utc, "Booking.CreatedAtUtc must be a UTC timestamp.");

        Result<Money> total = Quote(roomType.Id, stay, rooms, inventory, currency, today);
        if (total.IsFailure) return total.Error;

        string trimmedLeadGuestName = leadGuestName.Trim();
        string? trimmedNotes = TextNormalizer.TrimToNull(notes);
        string trimmedClientName = clientName.Trim();
        string? trimmedClientReference = TextNormalizer.TrimToNull(clientReference);
        int maxGuests = rooms * roomType.MaxOccupancy;

        if (trimmedLeadGuestName.Length == 0) return BookingErrors.LeadGuestNameRequired;
        if (trimmedLeadGuestName.Length > LEAD_GUEST_NAME_MAX_LENGTH) return BookingErrors.LeadGuestNameTooLong;
        if (trimmedNotes?.Length > NOTES_MAX_LENGTH) return BookingErrors.NotesTooLong;
        if (!Enum.IsDefined(channel)) return BookingErrors.ChannelInvalid;
        if (trimmedClientName.Length == 0) return BookingErrors.ClientNameRequired;
        if (trimmedClientName.Length > CLIENT_NAME_MAX_LENGTH) return BookingErrors.ClientNameTooLong;
        if (trimmedClientReference?.Length > CLIENT_REFERENCE_MAX_LENGTH) return BookingErrors.ClientReferenceTooLong;
        if (children < 0) return BookingErrors.ChildrenNegative;
        // Every room needs a responsible adult.
        if (adults < rooms) return BookingErrors.NotEnoughAdults;
        if (adults + children > maxGuests) return BookingErrors.TooManyGuests(rooms, maxGuests);
        if (total.Value != quotedTotal) return BookingErrors.PriceChanged;

        Dictionary<DateOnly, InventoryDay> inventoryByNight = InventoryByNight(roomType.Id, inventory);
        foreach (DateOnly night in stay.EachNight())
        {
            Result reserved = inventoryByNight[night].Reserve(rooms);
            Debug.Assert(reserved.IsSuccess, "The quote already checked every night, so reserving cannot fail.");
        }

        return new Booking(
            Guid.CreateVersion7(),
            GenerateReference(),
            roomType.HotelId,
            roomType.Id,
            rooms,
            adults,
            children,
            trimmedLeadGuestName,
            trimmedNotes,
            channel,
            trimmedClientName,
            trimmedClientReference,
            BookingStatus.Confirmed,
            createdAtUtc,
            null)
        {
            Stay = stay,
            TotalPrice = total.Value,
        };
    }

    public Result Cancel(IReadOnlyCollection<InventoryDay> inventory, DateOnly today, DateTime cancelledAtUtc)
    {
        Debug.Assert(cancelledAtUtc.Kind == DateTimeKind.Utc, "Booking.CancelledAtUtc must be a UTC timestamp.");

        if (Status == BookingStatus.Cancelled) return BookingErrors.AlreadyCancelled;
        if (Stay.CheckIn < today) return BookingErrors.CheckInPassed;

        Dictionary<DateOnly, InventoryDay> inventoryByNight = InventoryByNight(RoomTypeId, inventory);
        foreach (DateOnly night in Stay.EachNight())
        {
            inventoryByNight[night].Release(Rooms);
        }

        Status = BookingStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
        return Result.Success();
    }

    public void RenewReference() => Reference = GenerateReference();

    private static Dictionary<DateOnly, InventoryDay> InventoryByNight(Guid roomTypeId, IReadOnlyCollection<InventoryDay> inventory)
    {
        Debug.Assert(inventory.All(day => day.RoomTypeId == roomTypeId), "Inventory rows must belong to the booked room type.");

        return inventory.ToDictionary(day => day.Date);
    }

    private static string GenerateReference() =>
        REFERENCE_PREFIX + RandomNumberGenerator.GetString(REFERENCE_ALPHABET, REFERENCE_LENGTH - REFERENCE_PREFIX.Length);
}
