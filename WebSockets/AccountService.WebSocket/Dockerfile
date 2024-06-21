#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebSockets/AccountService.WebSocket/AccountService.WebSocket.csproj", "WebSockets/AccountService.WebSocket/"]
COPY ["Libraries/AccountService.Library/AccountService.Library.csproj", "Libraries/AccountService.Library/"]
RUN dotnet restore "WebSockets/AccountService.WebSocket/AccountService.WebSocket.csproj"
COPY . .
WORKDIR "/src/WebSockets/AccountService.WebSocket"
RUN dotnet build "AccountService.WebSocket.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccountService.WebSocket.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountService.WebSocket.dll"]
