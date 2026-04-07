FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Udemy.Shared/Udemy.Shared.csproj", "Udemy.Shared/"]
COPY ["Invoice/Udemy.Invoice.API/Udemy.Invoice.API.csproj", "Invoice/Udemy.Invoice.API/"]
RUN dotnet restore "Invoice/Udemy.Invoice.API/Udemy.Invoice.API.csproj"
COPY . .
WORKDIR "/src/Invoice/Udemy.Invoice.API"
RUN dotnet build "Udemy.Invoice.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Udemy.Invoice.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Udemy.Invoice.API.dll"]
