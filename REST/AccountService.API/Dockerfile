FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["REST/AccountService.API/AccountService.API.csproj", "REST/AccountService.API/"]
COPY ["Libraries/AccountService.Library/AccountService.Library.csproj", "Libraries/AccountService.Library/"]
RUN dotnet restore "REST/AccountService.API/AccountService.API.csproj"
COPY . .
WORKDIR "/src/REST/AccountService.API"
RUN dotnet build "AccountService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccountService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountService.API.dll"]
