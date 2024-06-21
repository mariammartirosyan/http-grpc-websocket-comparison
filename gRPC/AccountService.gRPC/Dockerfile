#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["gRPC/AccountService.gRPC/AccountService.gRPC.csproj", "gRPC/AccountService.gRPC/"]
COPY ["Libraries/AccountService.Library/AccountService.Library.csproj", "Libraries/AccountService.Library/"]
RUN dotnet restore "gRPC/AccountService.gRPC/AccountService.gRPC.csproj"
COPY . .
WORKDIR "/src/gRPC/AccountService.gRPC"
RUN dotnet build "AccountService.gRPC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccountService.gRPC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountService.gRPC.dll"]
