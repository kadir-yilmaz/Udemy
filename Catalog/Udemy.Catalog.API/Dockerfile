FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Udemy.Shared/Udemy.Shared.csproj", "Udemy.Shared/"]
COPY ["Catalog/Udemy.Catalog.API/Udemy.Catalog.API.csproj", "Catalog/Udemy.Catalog.API/"]
RUN dotnet restore "Catalog/Udemy.Catalog.API/Udemy.Catalog.API.csproj"
COPY . .
WORKDIR "/src/Catalog/Udemy.Catalog.API"
RUN dotnet build "Udemy.Catalog.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Udemy.Catalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Udemy.Catalog.API.dll"]
