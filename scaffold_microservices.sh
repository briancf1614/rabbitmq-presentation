#!/bin/bash

# Order Service
cd src/OrderService
dotnet new webapi -n OrderService.Api -o src/OrderService.Api --no-openapi
dotnet new classlib -n OrderService.Application -o src/OrderService.Application
dotnet new classlib -n OrderService.Domain -o src/OrderService.Domain
dotnet new classlib -n OrderService.Infrastructure -o src/OrderService.Infrastructure

dotnet new sln -n OrderService
dotnet sln add src/OrderService.Api/OrderService.Api.csproj
dotnet sln add src/OrderService.Application/OrderService.Application.csproj
dotnet sln add src/OrderService.Domain/OrderService.Domain.csproj
dotnet sln add src/OrderService.Infrastructure/OrderService.Infrastructure.csproj

cd src/OrderService.Api
dotnet add reference ../OrderService.Application/OrderService.Application.csproj
dotnet add reference ../OrderService.Infrastructure/OrderService.Infrastructure.csproj

cd ../OrderService.Application
dotnet add reference ../OrderService.Domain/OrderService.Domain.csproj

cd ../OrderService.Infrastructure
dotnet add reference ../OrderService.Application/OrderService.Application.csproj
cd ../../../

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
