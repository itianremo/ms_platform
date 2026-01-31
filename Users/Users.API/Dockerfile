FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Users/Users.API/Users.API.csproj", "Users/Users.API/"]
COPY ["Users/Users.Application/Users.Application.csproj", "Users/Users.Application/"]
COPY ["Users/Users.Domain/Users.Domain.csproj", "Users/Users.Domain/"]
COPY ["Users/Users.Infrastructure/Users.Infrastructure.csproj", "Users/Users.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "Users/Users.API/Users.API.csproj"
COPY . .
WORKDIR "/src/Users/Users.API"
RUN dotnet build "Users.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Users.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Users.API.dll"]
