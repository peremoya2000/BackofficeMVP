using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Application.Common;

public static class RequestErrors
{
    public static Error SortInvalid(string? sort) => Error.Validation(
        "request.sort_invalid",
        $"No se puede ordenar por '{sort}'.");
}
