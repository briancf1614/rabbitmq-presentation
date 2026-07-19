#!/bin/bash
# Inventory Service
cd src/InventoryService
dotnet new webapi -n InventoryService.Api -o src/InventoryService.Api --no-openapi
dotnet new classlib -n InventoryService.Application -o src/InventoryService.Application
dotnet new classlib -n InventoryService.Domain -o src/InventoryService.Domain
dotnet new classlib -n InventoryService.Infrastructure -o src/InventoryService.Infrastructure

dotnet new sln -n InventoryService
dotnet sln add src/InventoryService.Api/InventoryService.Api.csproj
dotnet sln add src/InventoryService.Application/InventoryService.Application.csproj
dotnet sln add src/InventoryService.Domain/InventoryService.Domain.csproj
dotnet sln add src/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj

cd src/InventoryService.Api
dotnet add reference ../InventoryService.Application/InventoryService.Application.csproj
dotnet add reference ../InventoryService.Infrastructure/InventoryService.Infrastructure.csproj

cd ../InventoryService.Application
dotnet add reference ../InventoryService.Domain/InventoryService.Domain.csproj

cd ../InventoryService.Infrastructure
dotnet add reference ../InventoryService.Application/InventoryService.Application.csproj
cd ../../../
