FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Udemy.Shared/Udemy.Shared.csproj", "Udemy.Shared/"]
COPY ["PhotoStock/Udemy.PhotoStock.API/Udemy.PhotoStock.API.csproj", "PhotoStock/Udemy.PhotoStock.API/"]
RUN dotnet restore "PhotoStock/Udemy.PhotoStock.API/Udemy.PhotoStock.API.csproj"
COPY . .
WORKDIR "/src/PhotoStock/Udemy.PhotoStock.API"
RUN dotnet build "Udemy.PhotoStock.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Udemy.PhotoStock.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Udemy.PhotoStock.API.dll"]
