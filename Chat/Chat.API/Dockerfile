FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Chat/Chat.API/Chat.API.csproj", "Chat/Chat.API/"]
COPY ["Chat/Chat.Application/Chat.Application.csproj", "Chat/Chat.Application/"]
COPY ["Chat/Chat.Domain/Chat.Domain.csproj", "Chat/Chat.Domain/"]
COPY ["Chat/Chat.Infrastructure/Chat.Infrastructure.csproj", "Chat/Chat.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]
RUN dotnet restore "Chat/Chat.API/Chat.API.csproj"
COPY . .
WORKDIR "/src/Chat/Chat.API"
RUN dotnet build "Chat.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chat.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chat.API.dll"]
