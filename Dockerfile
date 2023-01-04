#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Pondrop.Service.ShoppingList.Api/Pondrop.Service.ShoppingList.Api.csproj", "src/Pondrop.Service.ShoppingList.Api/"]
COPY ["src/Pondrop.Service.ShoppingList.Application/Pondrop.Service.ShoppingList.Application.csproj", "src/Pondrop.Service.ShoppingList.Application/"]
COPY ["src/Pondrop.Service.ShoppingList.Domain/Pondrop.Service.ShoppingList.Domain.csproj", "src/Pondrop.Service.ShoppingList.Domain/"]
COPY ["src/Pondrop.Service.ShoppingList.Infrastructure/Pondrop.Service.ShoppingList.Infrastructure.csproj", "src/Pondrop.Service.ShoppingList.Infrastructure/"]
RUN dotnet nuget add source "https://pkgs.dev.azure.com/PondropDevOps/_packaging/PondropDevOps/nuget/v3/index.json" --name "PondropInfrastructure" --username "user" --password "3sn7hxhu5n3jlg22cbojteotocsuccn257z5zqyat7btza6z4qbq" --store-password-in-clear-text
RUN dotnet restore "src/Pondrop.Service.ShoppingList.Api/Pondrop.Service.ShoppingList.Api.csproj"
COPY . .
WORKDIR "/src/src/Pondrop.Service.ShoppingList.Api"
RUN dotnet build "Pondrop.Service.ShoppingList.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pondrop.Service.ShoppingList.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pondrop.Service.ShoppingList.Api.dll"]