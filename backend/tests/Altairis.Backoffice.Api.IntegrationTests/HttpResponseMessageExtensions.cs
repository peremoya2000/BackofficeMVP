using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace Altairis.Backoffice.Api.IntegrationTests;

internal static class HttpResponseMessageExtensions
{
    public static async Task<string?> ReadErrorCodeAsync(this HttpResponseMessage response)
    {
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        return problem.Extensions["code"]?.ToString();
    }
}
