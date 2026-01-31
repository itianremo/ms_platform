FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Recommendation/Recommendation.API/Recommendation.API.csproj", "Recommendation/Recommendation.API/"]
COPY ["Recommendation/Recommendation.Application/Recommendation.Application.csproj", "Recommendation/Recommendation.Application/"]
COPY ["Recommendation/Recommendation.Domain/Recommendation.Domain.csproj", "Recommendation/Recommendation.Domain/"]
COPY ["Recommendation/Recommendation.Infrastructure/Recommendation.Infrastructure.csproj", "Recommendation/Recommendation.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
RUN dotnet restore "Recommendation/Recommendation.API/Recommendation.API.csproj"
COPY . .
WORKDIR "/src/Recommendation/Recommendation.API"
RUN dotnet build "Recommendation.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Recommendation.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Recommendation.API.dll"]
