FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Search/Search.API/Search.API.csproj", "Search/Search.API/"]
COPY ["Search/Search.Application/Search.Application.csproj", "Search/Search.Application/"]
COPY ["Search/Search.Domain/Search.Domain.csproj", "Search/Search.Domain/"]
COPY ["Search/Search.Infrastructure/Search.Infrastructure.csproj", "Search/Search.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "Search/Search.API/Search.API.csproj"
COPY . .
WORKDIR "/src/Search/Search.API"
RUN dotnet build "Search.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Search.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Search.API.dll"]
