FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Media/Media.API/Media.API.csproj", "Media/Media.API/"]
COPY ["Media/Media.Application/Media.Application.csproj", "Media/Media.Application/"]
COPY ["Media/Media.Infrastructure/Media.Infrastructure.csproj", "Media/Media.Infrastructure/"]
COPY ["Media/Media.Domain/Media.Domain.csproj", "Media/Media.Domain/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
RUN dotnet restore "Media/Media.API/Media.API.csproj"
COPY . .
WORKDIR "/src/Media/Media.API"
RUN dotnet build "Media.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Media.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Media.API.dll"]
