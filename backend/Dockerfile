# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore (camadas cacheáveis: copia apenas os manifests primeiro)
COPY Directory.Build.props ./
COPY src/SistemaAN.Domain/SistemaAN.Domain.csproj src/SistemaAN.Domain/
COPY src/SistemaAN.Application/SistemaAN.Application.csproj src/SistemaAN.Application/
COPY src/SistemaAN.Infrastructure/SistemaAN.Infrastructure.csproj src/SistemaAN.Infrastructure/
COPY src/SistemaAN.Api/SistemaAN.Api.csproj src/SistemaAN.Api/
RUN dotnet restore src/SistemaAN.Api/SistemaAN.Api.csproj

# Publish
COPY . .
RUN dotnet publish src/SistemaAN.Api/SistemaAN.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SistemaAN.Api.dll"]
