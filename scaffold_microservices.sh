#!/bin/bash
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
