namespace Altairis.Backoffice.Application.Common;

public static class LikePattern
{
    public const string ESCAPE_CHARACTER = @"\";

    public static string Containing(string term) => $"%{Escape(term)}%";

    private static string Escape(string term) => term
        .Replace(ESCAPE_CHARACTER, ESCAPE_CHARACTER + ESCAPE_CHARACTER)
        .Replace("%", ESCAPE_CHARACTER + "%")
        .Replace("_", ESCAPE_CHARACTER + "_");
}
