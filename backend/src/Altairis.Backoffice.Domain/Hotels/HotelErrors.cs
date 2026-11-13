using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Hotels;

public static class HotelErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "hotel.name_required",
        "El nombre del hotel es obligatorio.");

    public static readonly Error NameTooLong = Error.Validation(
        "hotel.name_too_long",
        $"El nombre del hotel no puede superar {Hotel.NAME_MAX_LENGTH} caracteres.");

    public static readonly Error StarsOutOfRange = Error.Validation(
        "hotel.stars_out_of_range",
        $"La categoría del hotel debe estar entre {Hotel.MIN_STARS} y {Hotel.MAX_STARS} estrellas.");

    public static readonly Error AddressRequired = Error.Validation(
        "hotel.address_required",
        "La dirección del hotel es obligatoria.");

    public static readonly Error AddressTooLong = Error.Validation(
        "hotel.address_too_long",
        $"La dirección del hotel no puede superar {Hotel.ADDRESS_MAX_LENGTH} caracteres.");

    public static readonly Error CityRequired = Error.Validation(
        "hotel.city_required",
        "La ciudad del hotel es obligatoria.");

    public static readonly Error CityTooLong = Error.Validation(
        "hotel.city_too_long",
        $"La ciudad del hotel no puede superar {Hotel.CITY_MAX_LENGTH} caracteres.");

    public static readonly Error CountryCodeInvalid = Error.Validation(
        "hotel.country_code_invalid",
        "El país debe ser un código ISO 3166-1 alfa-2 válido (por ejemplo, ES).");

    public static readonly Error ContactEmailInvalid = Error.Validation(
        "hotel.contact_email_invalid",
        "El email de contacto no es válido.");

    public static readonly Error ContactPhoneInvalid = Error.Validation(
        "hotel.contact_phone_invalid",
        "El teléfono de contacto no es válido.");
}
