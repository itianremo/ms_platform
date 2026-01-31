FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Scheduler/Scheduler.API/Scheduler.API.csproj", "Scheduler/Scheduler.API/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
RUN dotnet restore "Scheduler/Scheduler.API/Scheduler.API.csproj"
COPY . .
WORKDIR "/src/Scheduler/Scheduler.API"
RUN dotnet build "Scheduler.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Scheduler.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Scheduler.API.dll"]
