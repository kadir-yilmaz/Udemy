FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Udemy.Shared/Udemy.Shared.csproj", "Udemy.Shared/"]
COPY ["Order/Udemy.Order.API/Udemy.Order.API.csproj", "Order/Udemy.Order.API/"]
RUN dotnet restore "Order/Udemy.Order.API/Udemy.Order.API.csproj"
COPY . .
WORKDIR "/src/Order/Udemy.Order.API"
RUN dotnet build "Udemy.Order.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Udemy.Order.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Udemy.Order.API.dll"]
