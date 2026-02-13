FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY WeatherInfo.sln .
COPY WeatherInfo.API/ WeatherInfo.API/
COPY WeatherInfo.API.Tests/ WeatherInfo.API.Tests/

RUN dotnet restore

RUN dotnet test WeatherInfo.API.Tests/WeatherInfo.API.Tests.csproj

RUN dotnet publish WeatherInfo.API/WeatherInfo.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WeatherInfo.API.dll"]