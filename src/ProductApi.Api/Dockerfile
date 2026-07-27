FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ProductApi.Api/ProductApi.Api.csproj", "src/ProductApi.Api/"]
COPY ["src/ProductApi.Application/ProductApi.Application.csproj", "src/ProductApi.Application/"]
COPY ["src/ProductApi.Domain/ProductApi.Domain.csproj", "src/ProductApi.Domain/"]
COPY ["src/ProductApi.Infrastructure/ProductApi.Infrastructure.csproj", "src/ProductApi.Infrastructure/"]
RUN dotnet restore "src/ProductApi.Api/ProductApi.Api.csproj"
COPY . .
WORKDIR "/src/src/ProductApi.Api"
RUN dotnet build "./ProductApi.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProductApi.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductApi.Api.dll"]
