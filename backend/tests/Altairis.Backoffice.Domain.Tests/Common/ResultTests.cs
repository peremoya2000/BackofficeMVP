using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Tests.Common;

public class ResultTests
{
    private static readonly Error SampleError = Error.Conflict("sample.conflict", "Conflicto de ejemplo.");

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        Result<int> result = SampleError;

        Assert.True(result.IsFailure);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        Result<int> result = SampleError;

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Error_OnSuccess_Throws()
    {
        Result result = Result.Success();

        Assert.Throws<InvalidOperationException>(() => result.Error);
    }
}
