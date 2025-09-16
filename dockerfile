# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/MyApp.Web/MyApp.Web.csproj", "MyApp.Web/"]
COPY ["src/MyApp.Core/MyApp.Core.csproj", "MyApp.Core/"]
COPY ["src/MyApp.Infrastructure/MyApp.Infrastructure.csproj", "MyApp.Infrastructure/"]
RUN dotnet restore "MyApp.Web/MyApp.Web.csproj"

COPY . .
WORKDIR "/src/MyApp.Web"

# Publish tanpa runtime dan self-contained
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "MyApp.Web.dll"]
