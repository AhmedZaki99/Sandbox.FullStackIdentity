# syntax=docker/dockerfile:1

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to restore the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /repo/src
COPY ["src/Sandbox.FullStackIdentity.Services.Migrator/*.csproj", "Sandbox.FullStackIdentity.Services.Migrator/"]
WORKDIR /repo/src/Sandbox.FullStackIdentity.Services.Migrator
# Add private nuget source & restore.
RUN --mount=type=secret,id=GITHUB_USERNAME --mount=type=secret,id=GITHUB_TOKEN \
GITHUB_USERNAME=$(cat /run/secrets/GITHUB_USERNAME) && \
GITHUB_TOKEN=$(cat /run/secrets/GITHUB_TOKEN) && \
dotnet nuget add source --username $GITHUB_USERNAME --password $GITHUB_TOKEN --store-password-in-clear-text --name github "https://nuget.pkg.github.com/$GITHUB_USERNAME/index.json" && \
dotnet restore


# This stage is used to publish the service project to be copied to the final stage
FROM restore AS publish
ARG BUILD_CONFIGURATION=Release
# Copy source files.
WORKDIR /repo/src
COPY ["src/Sandbox.FullStackIdentity.Services.Migrator/", "Sandbox.FullStackIdentity.Services.Migrator/"]
# Publish the app binaries.
WORKDIR /repo/src/Sandbox.FullStackIdentity.Services.Migrator
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore


# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
# Expose the main entrypoint.
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sandbox.FullStackIdentity.Services.Migrator.dll"]
