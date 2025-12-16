# Multi-stage build for Convoy API - .NET 8.0

# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY Convoy.sln ./

# Copy all project files
COPY Convoy.Api/Convoy.Api.csproj Convoy.Api/
COPY Convoy.Service/Convoy.Service.csproj Convoy.Service/
COPY Convoy.Data/Convoy.Data.csproj Convoy.Data/
COPY Convoy.Domain/Convoy.Domain.csproj Convoy.Domain/

# Restore NuGet packages
RUN dotnet restore Convoy.sln

# Copy all source code
COPY . .

# Build and publish the API project
WORKDIR /src/Convoy.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Expose port for Railway/Render (they will map automatically)
EXPOSE 8080

# Set ASP.NET Core to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check (optional but recommended)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/swagger/index.html || exit 1

# Run the application
ENTRYPOINT ["dotnet", "Convoy.Api.dll"]
