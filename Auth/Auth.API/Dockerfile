FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Auth/Auth.API/Auth.API.csproj", "Auth/Auth.API/"]
COPY ["Auth/Auth.Application/Auth.Application.csproj", "Auth/Auth.Application/"]
COPY ["Auth/Auth.Domain/Auth.Domain.csproj", "Auth/Auth.Domain/"]
COPY ["Auth/Auth.Infrastructure/Auth.Infrastructure.csproj", "Auth/Auth.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "Auth/Auth.API/Auth.API.csproj"
COPY . .
WORKDIR "/src/Auth/Auth.API"
RUN dotnet build "Auth.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["Auth/Auth.API/seed-users.json", "."]
COPY ["Auth/Auth.API/seed-roles.json", "."]
COPY ["Auth/Auth.API/seed-permissions.json", "."]
ENTRYPOINT ["dotnet", "Auth.API.dll"]
