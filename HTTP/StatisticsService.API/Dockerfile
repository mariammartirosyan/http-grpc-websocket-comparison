#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HTTP/StatisticsService.API/StatisticsService.API.csproj", "HTTP/StatisticsService.API/"]
COPY ["Libraries/StatisticsService.Library/StatisticsService.Library.csproj", "Libraries/StatisticsService.Library/"]
RUN dotnet HTTPore "HTTP/StatisticsService.API/StatisticsService.API.csproj"
COPY . .
WORKDIR "/src/HTTP/StatisticsService.API"
RUN dotnet build "StatisticsService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatisticsService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StatisticsService.API.dll"]
