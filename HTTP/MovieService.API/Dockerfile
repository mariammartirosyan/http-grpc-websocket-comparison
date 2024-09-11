#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HTTP/MovieService.API/MovieService.API.csproj", "HTTP/MovieService.API/"]
COPY ["Libraries/MovieService.Library/MovieService.Library.csproj", "Libraries/MovieService.Library/"]
RUN dotnet HTTPore "HTTP/MovieService.API/MovieService.API.csproj"
COPY . .
WORKDIR "/src/HTTP/MovieService.API"
RUN dotnet build "MovieService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MovieService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MovieService.API.dll"]
