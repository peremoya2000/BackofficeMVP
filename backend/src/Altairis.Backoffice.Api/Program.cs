using System.Diagnostics;
using Altairis.Backoffice.Application;
using Altairis.Backoffice.Infrastructure;
using Altairis.Backoffice.Infrastructure.Persistence;
using Scalar.AspNetCore;

const string OPEN_API_ROUTE = "/api/openapi/{documentName}.json";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("Backoffice");
Debug.Assert(connectionString is not null, "The connection string 'ConnectionStrings:Backoffice' is missing.");

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services
    .AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.RespectNullableAnnotations = true;
        options.JsonSerializerOptions.RespectRequiredConstructorParameters = true;
    });
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddDbContextCheck<BackofficeDbContext>();

WebApplication app = builder.Build();

await app.Services.InitializeDatabaseAsync(CancellationToken.None);

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi(OPEN_API_ROUTE);
app.MapScalarApiReference("/api/docs", options => options.WithOpenApiRoutePattern(OPEN_API_ROUTE));
app.MapControllers();
app.MapHealthChecks("/api/health");

await app.RunAsync();
