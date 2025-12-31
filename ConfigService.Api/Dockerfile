# Use official .NET 8 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ConfigService.Api.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Use ASP.NET runtime image for final container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Start the app
ENTRYPOINT ["dotnet", "ConfigService.Api.dll"]
