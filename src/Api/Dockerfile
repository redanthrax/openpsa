# syntax=docker/dockerfile:1.7
# Multi-stage build for the OpenPSA API.
# Build context expected to be the repo root.

ARG DOTNET_VERSION=10.0
ARG RUNTIME_USER=appuser

# ─── Build stage ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy only the props/sln first to maximize layer caching for restore.
COPY OpenPsa.slnx Directory.Build.props Directory.Packages.props NuGet.config* ./
COPY src/ ./src/
COPY tests/ ./tests/

RUN dotnet restore src/Api/Api.csproj

RUN dotnet publish src/Api/Api.csproj \
      --configuration Release \
      --no-restore \
      --output /app \
      /p:UseAppHost=false

# ─── Runtime stage ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
ARG RUNTIME_USER
WORKDIR /app

# Run as non-root.
RUN useradd --uid 1001 --user-group --create-home --shell /sbin/nologin ${RUNTIME_USER}
COPY --from=build --chown=${RUNTIME_USER}:${RUNTIME_USER} /app .

USER ${RUNTIME_USER}

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=true

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Api.dll"]
