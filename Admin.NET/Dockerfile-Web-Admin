FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 20802

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["GameFrameX.Web.Entry/GameFrameX.Web.Entry.csproj", "GameFrameX.Web.Entry/"]
COPY ["GameFrameX.Web.Core/GameFrameX.Web.Core.csproj", "GameFrameX.Web.Core/"]
COPY ["GameFrameX.Application/GameFrameX.Application.csproj", "GameFrameX.Application/"]
COPY ["GameFrameX.Core/GameFrameX.Core.csproj", "GameFrameX.Core/"]
COPY ["GameFrameX.Core.Options/GameFrameX.Core.Options.csproj", "GameFrameX.Core.Options/"]
COPY ["GameFrameX.Core.Attribute/GameFrameX.Core.Attribute.csproj", "GameFrameX.Core.Attribute/"]
COPY ["GameFrameX.Entity/GameFrameX.Entity.csproj", "GameFrameX.Entity/"]
COPY ["GameFrameX.Core.Enum/GameFrameX.Core.Enum.csproj", "GameFrameX.Core.Enum/"]
COPY ["GameFrameX.Game.Enum/GameFrameX.Game.Enum.csproj", "GameFrameX.Game.Enum/"]
COPY ["GameFrameX.Entity.System/GameFrameX.Entity.System.csproj", "GameFrameX.Entity.System/"]
COPY ["GameFrameX.Core.BaseInputOutput/GameFrameX.Core.BaseInputOutput.csproj", "GameFrameX.Core.BaseInputOutput/"]
COPY ["GameFrameX.Core.Utility/GameFrameX.Core.Utility.csproj", "GameFrameX.Core.Utility/"]
COPY ["GameFrameX.Core.Base/GameFrameX.Core.Base.csproj", "GameFrameX.Core.Base/"]
COPY ["GameFrameX.Core.Extension/GameFrameX.Core.Extension.csproj", "GameFrameX.Core.Extension/"]
COPY ["GameFrameX.Entity.Client/GameFrameX.Entity.Client.csproj", "GameFrameX.Entity.Client/"]
COPY ["GameFrameX.Application.Game/GameFrameX.Application.Game.csproj", "GameFrameX.Application.Game/"]
COPY ["GameFrameX.Entity.Game/GameFrameX.Entity.Game.csproj", "GameFrameX.Entity.Game/"]
COPY ["GameFrameX.Application.Base/GameFrameX.Application.Base.csproj", "GameFrameX.Application.Base/"]
COPY ["GameFrameX.Global.Const/GameFrameX.Global.Const.csproj", "GameFrameX.Global.Const/"]
COPY ["GameFrameX.Application.Client/GameFrameX.Application.Client.csproj", "GameFrameX.Application.Client/"]
COPY ["GameFrameX.Application.System/GameFrameX.Application.System.csproj", "GameFrameX.Application.System/"]
RUN dotnet restore "GameFrameX.Web.Entry/GameFrameX.Web.Entry.csproj"
COPY . .
WORKDIR "/src/GameFrameX.Web.Entry"
RUN dotnet build "GameFrameX.Web.Entry.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameFrameX.Web.Entry.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameFrameX.Web.Entry.dll"]
