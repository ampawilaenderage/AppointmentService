# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Shared/Shared.csproj Shared/
COPY AppointmentService/AppointmentService.csproj AppointmentService/
RUN dotnet restore AppointmentService/AppointmentService.csproj

COPY Shared/ Shared/
COPY AppointmentService/ AppointmentService/
RUN dotnet publish AppointmentService/AppointmentService.csproj -c Release -o /app/publish

# Stage 2: Runtime (smaller image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AppointmentService.dll"]
