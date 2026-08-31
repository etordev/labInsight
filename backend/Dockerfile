# Build context should be the backend/ directory.
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY LabInsight.Api.csproj ./
RUN dotnet restore LabInsight.Api.csproj

COPY . ./
RUN dotnet publish LabInsight.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://0.0.0.0:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true \
    DOTNET_EnableDiagnostics=0

RUN adduser -D appuser

COPY --from=build --chown=appuser:appuser /app/publish ./

USER appuser
EXPOSE 8080

ENTRYPOINT ["dotnet", "LabInsight.Api.dll"]
