FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /source

COPY global.json ./
COPY Taskly.API/Taskly.API.csproj Taskly.API/
RUN dotnet restore Taskly.API/Taskly.API.csproj

COPY Taskly.API/ Taskly.API/
RUN dotnet publish Taskly.API/Taskly.API.csproj \
    --configuration "$BUILD_CONFIGURATION" \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "Taskly.API.dll"]
