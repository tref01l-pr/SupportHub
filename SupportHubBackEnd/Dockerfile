FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MailBridgeSupport.API/MailBridgeSupport.API.csproj", "MailBridgeSupport.Api/"]
COPY ["MailBridgeSupport.Application/MailBridgeSupport.Application.csproj", "MailBridgeSupport.Application/"]
COPY ["MailBridgeSupport.DataAccess.SqlServer/MailBridgeSupport.DataAccess.SqlServer.csproj", "MailBridgeSupport.DataAccess.SqlServer/"]
COPY ["MailBridgeSupport.Domain/MailBridgeSupport.Domain.csproj", "MailBridgeSupport.Domain/"]
COPY ["MailBridgeSupport.Infrastructure/MailBridgeSupport.Infrastructure.csproj", "MailBridgeSupport.Infrastructure/"]
RUN dotnet restore "MailBridgeSupport.Api/MailBridgeSupport.API.csproj"
COPY . .
WORKDIR "/src/MailBridgeSupport.API"
RUN dotnet build "MailBridgeSupport.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MailBridgeSupport.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MailBridgeSupport.API.dll"]
