FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Udemy.Shared/Udemy.Shared.csproj", "Udemy.Shared/"]
COPY ["Basket/Udemy.Basket.API/Udemy.Basket.API.csproj", "Basket/Udemy.Basket.API/"]
RUN dotnet restore "Basket/Udemy.Basket.API/Udemy.Basket.API.csproj"
COPY . .
WORKDIR "/src/Basket/Udemy.Basket.API"
RUN dotnet build "Udemy.Basket.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Udemy.Basket.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Udemy.Basket.API.dll"]
