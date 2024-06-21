#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["gRPC/TrailerStreamingService.gRPC/TrailerStreamingService.gRPC.csproj", "gRPC/TrailerStreamingService.gRPC/"]
COPY ["Libraries/TrailerStreamingService.Library/TrailerStreamingService.Library.csproj", "Libraries/TrailerStreamingService.Library/"]
RUN dotnet restore "gRPC/TrailerStreamingService.gRPC/TrailerStreamingService.gRPC.csproj"
COPY . .
WORKDIR "/src/gRPC/TrailerStreamingService.gRPC"
RUN dotnet build "TrailerStreamingService.gRPC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrailerStreamingService.gRPC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrailerStreamingService.gRPC.dll"]
