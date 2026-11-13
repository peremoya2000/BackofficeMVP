using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Tests.Common;

public class MoneyTests
{
    [Fact]
    public void Add_WithDifferentCurrencies_Throws()
    {
        Money euros = new(100m, "EUR");
        Money dollars = new(100m, "USD");

        Assert.Throws<InvalidOperationException>(() => euros + dollars);
    }

    [Fact]
    public void Add_WithSameCurrency_ReturnsSum()
    {
        Money euros1 = new(100m, "EUR");
        Money euros2 = new(20.50m, "EUR");

        Money result = euros1 + euros2;

        Assert.Equal(new Money(120.50m, "EUR"), result);
    }

    [Fact]
    public void Multiply_ByFactor_MultipliesAndKeepsCurrency()
    {
        Money euros = new(100m, "EUR");
        int factor = 3;

        Money result = euros * factor;

        Assert.Equal(new Money(300m, "EUR"), result);
    }
}
