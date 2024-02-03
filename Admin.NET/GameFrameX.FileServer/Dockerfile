FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GameFrameX.FileServer/GameFrameX.FileServer.csproj", "GameFrameX.FileServer/"]
RUN dotnet restore "GameFrameX.FileServer/GameFrameX.FileServer.csproj"
COPY . .
WORKDIR "/src/GameFrameX.FileServer"
RUN dotnet build "GameFrameX.FileServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GameFrameX.FileServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameFrameX.FileServer.dll"]
