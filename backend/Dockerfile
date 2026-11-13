FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Altairis.Backoffice.Domain/Altairis.Backoffice.Domain.csproj src/Altairis.Backoffice.Domain/
COPY src/Altairis.Backoffice.Contracts/Altairis.Backoffice.Contracts.csproj src/Altairis.Backoffice.Contracts/
COPY src/Altairis.Backoffice.Application/Altairis.Backoffice.Application.csproj src/Altairis.Backoffice.Application/
COPY src/Altairis.Backoffice.Infrastructure/Altairis.Backoffice.Infrastructure.csproj src/Altairis.Backoffice.Infrastructure/
COPY src/Altairis.Backoffice.Api/Altairis.Backoffice.Api.csproj src/Altairis.Backoffice.Api/
RUN dotnet restore src/Altairis.Backoffice.Api/Altairis.Backoffice.Api.csproj

COPY src/ src/
RUN dotnet publish src/Altairis.Backoffice.Api/Altairis.Backoffice.Api.csproj --configuration Release --no-restore --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Altairis.Backoffice.Api.dll"]
