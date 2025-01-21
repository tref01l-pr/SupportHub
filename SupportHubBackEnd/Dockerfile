FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SupportHubBackEnd/SupportHub.API/SupportHub.API.csproj", "SupportHub.Api/"]
COPY ["SupportHubBackEnd/SupportHub.Application/SupportHub.Application.csproj", "SupportHub.Application/"]
COPY ["SupportHubBackEnd/SupportHub.DataAccess.SqlServer/SupportHub.DataAccess.SqlServer.csproj", "SupportHub.DataAccess.SqlServer/"]
COPY ["SupportHubBackEnd/SupportHub.Domain/SupportHub.Domain.csproj", "SupportHub.Domain/"]
COPY ["SupportHubBackEnd/SupportHub.Infrastructure/SupportHub.Infrastructure.csproj", "SupportHub.Infrastructure/"]
RUN dotnet restore "SupportHubBackEnd/SupportHub.Api/SupportHub.API.csproj"
COPY . .
WORKDIR "/src/SupportHub.API"
RUN dotnet build "SupportHub.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SupportHub.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SupportHub.API.dll"]
