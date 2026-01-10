FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Apps/Apps.API/Apps.API.csproj", "Apps/Apps.API/"]
COPY ["Apps/Apps.Application/Apps.Application.csproj", "Apps/Apps.Application/"]
COPY ["Apps/Apps.Domain/Apps.Domain.csproj", "Apps/Apps.Domain/"]
COPY ["Apps/Apps.Infrastructure/Apps.Infrastructure.csproj", "Apps/Apps.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
RUN dotnet restore "Apps/Apps.API/Apps.API.csproj"
COPY . .
WORKDIR "/src/Apps/Apps.API"
RUN dotnet build "Apps.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Apps.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Apps.API.dll"]
