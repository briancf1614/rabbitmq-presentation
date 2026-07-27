FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/CatalogService.Api/CatalogService.Api.csproj", "src/CatalogService.Api/"]
RUN dotnet restore "src/CatalogService.Api/CatalogService.Api.csproj"
COPY . .
WORKDIR "/src/src/CatalogService.Api"
RUN dotnet build "CatalogService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatalogService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatalogService.Api.dll"]
