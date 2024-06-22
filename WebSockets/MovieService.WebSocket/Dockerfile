#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebSockets/MovieService.WebSocket/MovieService.WebSocket.csproj", "WebSockets/MovieService.WebSocket/"]
COPY ["Libraries/MovieService.Library/MovieService.Library.csproj", "Libraries/MovieService.Library/"]
RUN dotnet restore "WebSockets/MovieService.WebSocket/MovieService.WebSocket.csproj"
COPY . .
WORKDIR "/src/WebSockets/MovieService.WebSocket"
RUN dotnet build "MovieService.WebSocket.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MovieService.WebSocket.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MovieService.WebSocket.dll"]
