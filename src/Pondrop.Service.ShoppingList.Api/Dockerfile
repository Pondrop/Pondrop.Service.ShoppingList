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