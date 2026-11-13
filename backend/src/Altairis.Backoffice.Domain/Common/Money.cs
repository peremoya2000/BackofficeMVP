namespace Altairis.Backoffice.Domain.Common;

public readonly record struct Money(decimal Amount, string Currency)
{
    public const int CURRENCY_CODE_LENGTH = 3;

    public static Money Zero(string currency) => new Money(0m, currency);

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException("Cannot add money with different currencies.");
        }

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator *(Money money, int factor)
    {
        return new Money(money.Amount * factor, money.Currency);
    }
}
