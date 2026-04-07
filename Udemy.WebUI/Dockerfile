FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Udemy.Shared/Udemy.Shared.csproj", "Udemy.Shared/"]
COPY ["Udemy.WebUI/Udemy.WebUI.csproj", "Udemy.WebUI/"]
RUN dotnet restore "Udemy.WebUI/Udemy.WebUI.csproj"
COPY . .
WORKDIR "/src/Udemy.WebUI"
RUN dotnet build "Udemy.WebUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Udemy.WebUI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Udemy.WebUI.dll"]
