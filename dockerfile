# ======================
#  BUILD STAGE
# ======================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy hanya file csproj (agar restore bisa di-cache)
COPY src/MyApp.Web/*.csproj src/MyApp.Web/
COPY src/MyApp.Core/*.csproj src/MyApp.Core/
COPY src/MyApp.Infrastructure/*.csproj src/MyApp.Infrastructure/
COPY MyApp.sln .

# Restore dependencies
RUN dotnet restore ./MyApp.sln

# Copy seluruh source code setelah restore
COPY . .

# Build & publish project web
RUN dotnet publish src/MyApp.Web/MyApp.Web.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

# ======================
#  RUNTIME STAGE
# ======================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "MyApp.Web.dll"]
