FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/obligatorio.WebApi/obligatorio.WebApi.csproj src/obligatorio.WebApi/
COPY src/DataAccess/DataAccess.csproj src/DataAccess/
COPY src/IDataAccess/IDataAccess.csproj src/IDataAccess/
COPY src/Domain/Domain.csproj src/Domain/
RUN dotnet restore src/obligatorio.WebApi/obligatorio.WebApi.csproj

COPY . .
RUN dotnet publish src/obligatorio.WebApi/obligatorio.WebApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "obligatorio.WebApi.dll"]
