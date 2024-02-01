FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["GameFrameX.Web.Api/GameFrameX.Web.Api.csproj", "GameFrameX.Web.Api/"]
COPY ["Api/GameFrameX.Application.Client.Api/GameFrameX.Application.Client.Api.csproj", "Api/GameFrameX.Application.Client.Api/"]
COPY ["GameFrameX.Core.Extension/GameFrameX.Core.Extension.csproj", "GameFrameX.Core.Extension/"]
COPY ["GameFrameX.Core.Base/GameFrameX.Core.Base.csproj", "GameFrameX.Core.Base/"]
COPY ["GameFrameX.Core.BaseInputOutput/GameFrameX.Core.BaseInputOutput.csproj", "GameFrameX.Core.BaseInputOutput/"]
COPY ["GameFrameX.Entity/GameFrameX.Entity.csproj", "GameFrameX.Entity/"]
COPY ["GameFrameX.Core.Attribute/GameFrameX.Core.Attribute.csproj", "GameFrameX.Core.Attribute/"]
COPY ["GameFrameX.Core.Enum/GameFrameX.Core.Enum.csproj", "GameFrameX.Core.Enum/"]
COPY ["GameFrameX.Game.Enum/GameFrameX.Game.Enum.csproj", "GameFrameX.Game.Enum/"]
COPY ["GameFrameX.Core/GameFrameX.Core.csproj", "GameFrameX.Core/"]
COPY ["GameFrameX.Core.Options/GameFrameX.Core.Options.csproj", "GameFrameX.Core.Options/"]
COPY ["GameFrameX.Entity.System/GameFrameX.Entity.System.csproj", "GameFrameX.Entity.System/"]
COPY ["GameFrameX.Core.Utility/GameFrameX.Core.Utility.csproj", "GameFrameX.Core.Utility/"]
COPY ["GameFrameX.Entity.Client/GameFrameX.Entity.Client.csproj", "GameFrameX.Entity.Client/"]
RUN dotnet restore "GameFrameX.Web.Api/GameFrameX.Web.Api.csproj"
COPY . .
WORKDIR "/src/GameFrameX.Web.Api"
RUN dotnet build "GameFrameX.Web.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameFrameX.Web.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameFrameX.Web.Api.dll"]
