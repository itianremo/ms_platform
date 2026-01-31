FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Geo/Geo.API/Geo.API.csproj", "Geo/Geo.API/"]
COPY ["Geo/Geo.Application/Geo.Application.csproj", "Geo/Geo.Application/"]
COPY ["Geo/Geo.Domain/Geo.Domain.csproj", "Geo/Geo.Domain/"]
COPY ["Geo/Geo.Infrastructure/Geo.Infrastructure.csproj", "Geo/Geo.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "Geo/Geo.API/Geo.API.csproj"
COPY . .
WORKDIR "/src/Geo/Geo.API"
RUN dotnet build "Geo.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Geo.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Geo.API.dll"]
