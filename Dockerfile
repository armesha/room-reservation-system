# Use .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY RoomReservationSystem.csproj ./
RUN dotnet restore

# Copy remaining files and build the application
COPY . ./
RUN dotnet publish RoomReservationSystem.csproj -c Release -o out

# Use a lightweight runtime image for deployment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose port 80 for external access
EXPOSE 80

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "RoomReservationSystem.dll"]
