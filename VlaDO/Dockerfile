# Сборка проекта
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY VlaDO.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Запуск API
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Используем SQLite-файл в отдельной папке /data
ENV ConnectionStrings__DefaultConnection="Data Source=/data/VlaDO.db"
RUN mkdir /data
VOLUME /data

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "VlaDO.dll"]
