FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore
RUN dotnet build "YoutubeTelegramMusic.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YoutubeTelegramMusic.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get -y update; apt-get -y install curl wget xz-utils python3

RUN curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp > yt-dlp
RUN chmod +x yt-dlp
ENV YOUTUBE_DLP_PATH=/app/yt-dlp

RUN wget https://johnvansickle.com/ffmpeg/builds/ffmpeg-git-amd64-static.tar.xz
RUN tar xvf ffmpeg-git-amd64-static.tar.xz
RUN FFMPEG_FOLDER="$(tar -tf ffmpeg-git-amd64-static.tar.xz | head -1 | cut -f1 -d"/")"
ENV FFMPEG_PATH="/app/$FFMPEG_FOLDER/ffmpeg"

ENTRYPOINT ["dotnet", "YoutubeTelegramMusic.dll"]
