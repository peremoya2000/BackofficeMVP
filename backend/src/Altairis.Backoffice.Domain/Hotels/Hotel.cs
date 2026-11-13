using System.Diagnostics;
using System.Net.Mail;
using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Hotels;

public sealed class Hotel
{
    public const int NAME_MAX_LENGTH = 200;
    public const int ADDRESS_MAX_LENGTH = 250;
    public const int CITY_MAX_LENGTH = 100;
    public const int CONTACT_EMAIL_MAX_LENGTH = 254;
    public const int CONTACT_PHONE_MAX_LENGTH = 30;
    public const int MIN_STARS = 1;
    public const int MAX_STARS = 5;
    public const string MVP_CURRENCY = "EUR";

    private const int CONTACT_PHONE_MIN_DIGITS = 6;
    private const string CONTACT_PHONE_SYMBOLS = "+-(). ";

    private readonly List<RoomType> _roomTypes = [];

    private Hotel(
        Guid id,
        string name,
        int stars,
        string address,
        string city,
        string countryCode,
        string currency,
        string? contactEmail,
        string? contactPhone,
        DateTime createdAtUtc)
    {
        Id = id;
        Name = name;
        Stars = stars;
        Address = address;
        City = city;
        CountryCode = countryCode;
        Currency = currency;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public int Stars { get; private set; }

    public string Address { get; private set; }

    public string City { get; private set; }

    public string CountryCode { get; private set; }

    public string Currency { get; private set; }

    public string? ContactEmail { get; private set; }

    public string? ContactPhone { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyList<RoomType> RoomTypes => _roomTypes.AsReadOnly();

    public static Result<Hotel> Create(
        string name,
        int stars,
        string address,
        string city,
        string countryCode,
        string? contactEmail,
        string? contactPhone,
        DateTime createdAtUtc)
    {
        Debug.Assert(createdAtUtc.Kind == DateTimeKind.Utc, "Hotel.CreatedAtUtc must be a UTC timestamp.");

        string trimmedName = name.Trim();
        string trimmedAddress = address.Trim();
        string trimmedCity = city.Trim();
        string normalizedCountryCode = countryCode.Trim().ToUpperInvariant();
        string? trimmedEmail = TextNormalizer.TrimToNull(contactEmail);
        string? trimmedPhone = TextNormalizer.TrimToNull(contactPhone);

        if (trimmedName.Length == 0) return HotelErrors.NameRequired;
        if (trimmedName.Length > NAME_MAX_LENGTH) return HotelErrors.NameTooLong;
        if (stars < MIN_STARS || stars > MAX_STARS) return HotelErrors.StarsOutOfRange;
        if (trimmedAddress.Length == 0) return HotelErrors.AddressRequired;
        if (trimmedAddress.Length > ADDRESS_MAX_LENGTH) return HotelErrors.AddressTooLong;
        if (trimmedCity.Length == 0) return HotelErrors.CityRequired;
        if (trimmedCity.Length > CITY_MAX_LENGTH) return HotelErrors.CityTooLong;
        if (!IsoCountryCodes.IsAssigned(normalizedCountryCode)) return HotelErrors.CountryCodeInvalid;
        if (trimmedEmail is not null && !IsValidEmail(trimmedEmail)) return HotelErrors.ContactEmailInvalid;
        if (trimmedPhone is not null && !IsValidPhone(trimmedPhone)) return HotelErrors.ContactPhoneInvalid;

        return new Hotel(
            Guid.CreateVersion7(),
            trimmedName,
            stars,
            trimmedAddress,
            trimmedCity,
            normalizedCountryCode,
            MVP_CURRENCY,
            trimmedEmail,
            trimmedPhone,
            createdAtUtc);
    }

    public Result<RoomType> AddRoomType(string name, RoomCategory category, int maxOccupancy)
    {
        Result<RoomType> created = RoomType.Create(Id, name, category, maxOccupancy);
        if (created.IsFailure) return created;

        RoomType roomType = created.Value;
        if (_roomTypes.Exists(existing => existing.HasName(roomType.Name))) return RoomTypeErrors.DuplicateName(roomType.Name);

        _roomTypes.Add(roomType);
        return roomType;
    }

    private static bool IsValidEmail(string email) =>
        email.Length <= CONTACT_EMAIL_MAX_LENGTH
        && MailAddress.TryCreate(email, out MailAddress? parsed)
        && parsed.Address == email;

    private static bool IsValidPhone(string phone) =>
        phone.Length <= CONTACT_PHONE_MAX_LENGTH
        && phone.All(character => char.IsAsciiDigit(character) || CONTACT_PHONE_SYMBOLS.Contains(character))
        && phone.Count(char.IsAsciiDigit) >= CONTACT_PHONE_MIN_DIGITS;
}
