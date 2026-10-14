FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY global.json ./
COPY src/ ./src/
RUN dotnet restore src/Schools/Mdaresna.Schools.Api/Mdaresna.Schools.Api.csproj
RUN dotnet publish src/Schools/Mdaresna.Schools.Api/Mdaresna.Schools.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl poppler-utils \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080

COPY --from=build /app/publish .
RUN mkdir -p /data/school-documents && chown "$APP_UID:$APP_UID" /data/school-documents
USER $APP_UID
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD test -w /data/school-documents && pdfinfo -v >/dev/null 2>&1 && curl --fail --silent --max-time 4 http://127.0.0.1:8080/health/ready || exit 1

ENTRYPOINT ["dotnet", "Mdaresna.Schools.Api.dll"]
