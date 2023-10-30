# Youtube Audio Downloader

This telegram bot can extract audio by YouTube link and upload it to [Telegram](https://telegram.org/) messenger

Just send link to a bot and get an audio file

![Docker Pulls](https://img.shields.io/docker/pulls/xolli/yt-tg-music?style=flat-square)
![Code Size](https://img.shields.io/github/languages/code-size/xolli/YoutubeTelegramMusic.svg?style=flat-square)
![License](https://img.shields.io/github/license/xolli/YoutubeTelegramMusic.svg?style=flat-square)

# Deploying

## Docker

[Docker image](https://hub.docker.com/r/xolli/yt-tg-music)

```bash
docker start -e 'TELEGRAM_BOT_TOKEN=<your token>' ... <other options> ... xolli/yt-tg-music
```

# Demo

You can try this bot on the [link](https://t.me/youtubedlmusicbot)

# Environment variables

| variable             | meaning                                                 | necessary | Default value            |
|----------------------|---------------------------------------------------------|-----------|--------------------------|
| `TELEGRAM_BOT_TOKEN` | Telegram bot token from @BotFather                      | true      |                          |
| `ADMIN_TG_IDS`       | Telegram user id list of admins                         | false     | null                     |
| `YT_PS_USERNAME`     | Postgres username to save statistic                     | false     | postgres                 |
| `YT_PS_PASSWORD`     | Postgres password to save statistic                     | false     | Empty password           |
| `YT_PS_HOSTNAME`     | Postgres hostname to save statistic                     | false     | localhost                |
| `YT_PS_DATABASE`     | Postgres database name to save statistic                | false     | same as `YT_PS_USERNAME` |
| `YOUTUBE_DLP_PATH`   | Path to [youtube dlp](https://github.com/yt-dlp/yt-dlp) | true      |                          |
| `FFMPEG_PATH`        | path to [ffmpeg](https://ffmpeg.org/)                   | true      |                          |
| `YT_LOG_DIRECTORY`   | path to log files                                       | false     | null                     |

# Planned features

- Video downloading
- Accessing by token
