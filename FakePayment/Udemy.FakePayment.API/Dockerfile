FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Udemy.Shared/Udemy.Shared.csproj", "Udemy.Shared/"]
COPY ["FakePayment/Udemy.FakePayment.API/Udemy.FakePayment.API.csproj", "FakePayment/Udemy.FakePayment.API/"]
RUN dotnet restore "FakePayment/Udemy.FakePayment.API/Udemy.FakePayment.API.csproj"
COPY . .
WORKDIR "/src/FakePayment/Udemy.FakePayment.API"
RUN dotnet build "Udemy.FakePayment.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Udemy.FakePayment.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Udemy.FakePayment.API.dll"]
