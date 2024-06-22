#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebSockets/StatisticsService.WebSocket/StatisticsService.WebSocket.csproj", "WebSockets/StatisticsService.WebSocket/"]
COPY ["Libraries/StatisticsService.Library/StatisticsService.Library.csproj", "Libraries/StatisticsService.Library/"]
RUN dotnet restore "WebSockets/StatisticsService.WebSocket/StatisticsService.WebSocket.csproj"
COPY . .
WORKDIR "/src/WebSockets/StatisticsService.WebSocket"
RUN dotnet build "StatisticsService.WebSocket.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatisticsService.WebSocket.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StatisticsService.WebSocket.dll"]
