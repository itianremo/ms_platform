FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Notifications/Notifications.API/Notifications.API.csproj", "Notifications/Notifications.API/"]
COPY ["Notifications/Notifications.Application/Notifications.Application.csproj", "Notifications/Notifications.Application/"]
COPY ["Notifications/Notifications.Domain/Notifications.Domain.csproj", "Notifications/Notifications.Domain/"]
COPY ["Notifications/Notifications.Infrastructure/Notifications.Infrastructure.csproj", "Notifications/Notifications.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "Notifications/Notifications.API/Notifications.API.csproj"
COPY . .
WORKDIR "/src/Notifications/Notifications.API"
RUN dotnet build "Notifications.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notifications.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notifications.API.dll"]
