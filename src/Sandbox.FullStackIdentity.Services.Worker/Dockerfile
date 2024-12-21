# syntax=docker/dockerfile:1

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# This stage is used to restore the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /repo
COPY ["src/*/*.csproj", "src/"]
WORKDIR /repo/src/Sandbox.FullStackIdentity.Services.Worker
RUN dotnet restore 


# This stage is used as a part of testing workflows
FROM restore AS test
ARG BUILD_CONFIGURATION=Release
# Restore test projects.
WORKDIR /repo
COPY ["tests/*/*.csproj", "tests/"]
WORKDIR /repo/tests/Sandbox.FullStackIdentity.Tests.Worker
RUN dotnet restore
# Copy solution files.
WORKDIR /repo
COPY ["src/*", "src/"]
COPY ["tests/*", "tests/"]
# Build project and expose test endpoint.
WORKDIR /repo/tests/Sandbox.FullStackIdentity.Tests.Worker
RUN dotnet build -c $BUILD_CONFIGURATION --no-restore
ENTRYPOINT ["dotnet", "test", "-c", "Release", "--verbosity", "normal", "--no-build"]


# This stage is used to publish the service project to be copied to the final stage
FROM restore AS publish
ARG BUILD_CONFIGURATION=Release
# Copy source files.
WORKDIR /repo
COPY ["src/*", "src/"]
# Publish the app binaries.
WORKDIR /repo/src/Sandbox.FullStackIdentity.Services.Worker
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore


# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
# Expose the main entrypoint.
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sandbox.FullStackIdentity.Services.Worker.dll"]
