# ── Stage 1: Build & Publish ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solution e projetos primeiro (melhor cache das camadas)
COPY ListService.slnx ./
COPY ListService/ListService.csproj ListService/
COPY ListService.Tests/ListService.Tests.csproj ListService.Tests/

RUN dotnet restore

# Copiar todo o código e publicar
COPY . .
RUN dotnet publish ListService/ListService.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ListService.dll"]
