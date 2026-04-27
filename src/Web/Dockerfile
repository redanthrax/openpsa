# syntax=docker/dockerfile:1.7
# Multi-stage build for the OpenPSA Blazor WASM Web app.
# Output is static files served by nginx.

ARG DOTNET_VERSION=10.0

# ─── Build stage ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY OpenPsa.slnx Directory.Build.props Directory.Packages.props NuGet.config* ./
COPY src/ ./src/

RUN dotnet restore src/Web/Web.csproj

RUN dotnet publish src/Web/Web.csproj \
      --configuration Release \
      --no-restore \
      --output /app

# ─── Runtime stage ──────────────────────────────────────────────────
FROM nginx:1.27-alpine AS runtime

COPY --from=build /app/wwwroot /usr/share/nginx/html
COPY src/Web/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD wget -qO- http://localhost:8080/ || exit 1
