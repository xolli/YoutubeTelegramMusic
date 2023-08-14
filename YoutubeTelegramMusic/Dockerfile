FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["YoutubeTelegramMusic/YoutubeTelegramMusic.csproj", "YoutubeTelegramMusic/"]
RUN dotnet restore "YoutubeTelegramMusic/YoutubeTelegramMusic.csproj"
COPY . .
WORKDIR "/src/YoutubeTelegramMusic"
RUN dotnet build "YoutubeTelegramMusic.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YoutubeTelegramMusic.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YoutubeTelegramMusic.dll"]
