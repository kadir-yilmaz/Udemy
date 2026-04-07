FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Udemy.Shared/Udemy.Shared.csproj", "Udemy.Shared/"]
COPY ["Udemy.NewIdentityServer/Udemy.NewIdentityServer.csproj", "Udemy.NewIdentityServer/"]
RUN dotnet restore "Udemy.NewIdentityServer/Udemy.NewIdentityServer.csproj"
COPY . .
WORKDIR "/src/Udemy.NewIdentityServer"
RUN dotnet build "Udemy.NewIdentityServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Udemy.NewIdentityServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Udemy.NewIdentityServer.dll"]
