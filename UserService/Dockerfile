# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Shared/Shared.csproj Shared/
COPY UserService/UserService.csproj UserService/
RUN dotnet restore UserService/UserService.csproj

COPY Shared/ Shared/
COPY UserService/ UserService/
RUN dotnet publish UserService/UserService.csproj -c Release -o /app/publish

# Stage 2: Runtime (smaller image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserService.dll"]
