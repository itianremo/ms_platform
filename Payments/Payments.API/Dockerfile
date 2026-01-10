FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Payments/Payments.API/Payments.API.csproj", "Payments/Payments.API/"]
COPY ["Payments/Payments.Application/Payments.Application.csproj", "Payments/Payments.Application/"]
COPY ["Payments/Payments.Domain/Payments.Domain.csproj", "Payments/Payments.Domain/"]
COPY ["Payments/Payments.Infrastructure/Payments.Infrastructure.csproj", "Payments/Payments.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "Payments/Payments.API/Payments.API.csproj"
COPY . .
WORKDIR "/src/Payments/Payments.API"
RUN dotnet build "Payments.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Payments.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payments.API.dll"]
