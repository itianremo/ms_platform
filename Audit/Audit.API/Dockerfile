FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Audit/Audit.API/Audit.API.csproj", "Audit/Audit.API/"]
COPY ["Audit/Audit.Application/Audit.Application.csproj", "Audit/Audit.Application/"]
COPY ["Audit/Audit.Domain/Audit.Domain.csproj", "Audit/Audit.Domain/"]
COPY ["Audit/Audit.Infrastructure/Audit.Infrastructure.csproj", "Audit/Audit.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "Audit/Audit.API/Audit.API.csproj"
COPY . .
WORKDIR "/src/Audit/Audit.API"
RUN dotnet build "Audit.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Audit.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Audit.API.dll"]
