FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Udemy.Shared/Udemy.Shared.csproj", "Udemy.Shared/"]
COPY ["Udemy.Gateway/Udemy.Gateway.csproj", "Udemy.Gateway/"]
RUN dotnet restore "Udemy.Gateway/Udemy.Gateway.csproj"
COPY . .
WORKDIR "/src/Udemy.Gateway"
RUN dotnet build "Udemy.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Udemy.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Udemy.Gateway.dll"]
