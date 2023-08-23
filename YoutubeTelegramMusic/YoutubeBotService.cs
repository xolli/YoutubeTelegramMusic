using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace YoutubeTelegramMusic;

public class YoutubeBotService : IHostedService
{
    private const string TelegramEnv = "TELEGRAM_BOT_TOKEN";
    private const string YtDlpEnv = "YOUTUBE_DLP_PATH";
    private const string FfmpegEnv = "FFMPEG_PATH";
    private const long TelegramFileSizeLimit = 50 * 1024 * 1024;

    private readonly TelegramBotClient _botClient;
    private readonly YoutubeDL _ytdl;
    private readonly CancellationTokenSource _cts;

    public YoutubeBotService()
    {
        var token = Environment.GetEnvironmentVariable(TelegramEnv);
        if (token == null)
        {
            throw new EnvVariablesException($"Expect Telegram token. Set it to environment variable {TelegramEnv}");
        }

        _botClient = new TelegramBotClient(token);
        string? fFmpegPath = Environment.GetEnvironmentVariable(FfmpegEnv);
        string? youtubeDlPath = Environment.GetEnvironmentVariable(YtDlpEnv);
        if (fFmpegPath == null)
        {
            throw new EnvVariablesException($"Expect ffmpeg path. Set it to environment variable {FfmpegEnv}");
        }

        if (youtubeDlPath == null)
        {
            throw new EnvVariablesException($"Expect yt-dlp path. Set it to environment variable {YtDlpEnv}");
        }

        _ytdl = new YoutubeDL
        {
            YoutubeDLPath = youtubeDlPath,
            FFmpegPath = fFmpegPath,
            OutputFolder = "/tmp"
        };
        _cts = new CancellationTokenSource();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        var options = new OptionSet()
        {
            EmbedMetadata = true
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token
        );

        return Task.CompletedTask;

        async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancelToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
            {
                return;
            }

            // Only process text messages
            if (message.Text is not { } messageText)
            {
                return;
            }

            var chatId = message.Chat.Id;
            if (!Util.IsYoutubeLink(messageText))
            {
                await client.SendTextMessageAsync(
                    chatId: chatId,
                    text: "It is not a YouTube link",
                    cancellationToken: cancelToken);
                return;
            }

            var updateMessage = await client.SendTextMessageAsync(chatId: chatId, text: "Downloading from youtube...",
                cancellationToken: cancelToken);
            var startUploading = false;

            var progress = new Progress<DownloadProgress>(UpdateProgress);

            var audioData = await _ytdl.RunVideoDataFetch(messageText, overrideOptions: options, ct: cancelToken);
            if (!audioData.Success)
            {
                await client.EditMessageTextAsync(chatId: chatId,
                    messageId: updateMessage.MessageId, text: $"{string.Join(", ", audioData.ErrorOutput)}",
                    cancellationToken: cancelToken);
                return;
            }

            var res = await _ytdl.RunAudioDownload(
                messageText,
                AudioConversionFormat.Mp3,
                progress: progress,
                ct: cancelToken);

            if (!res.Success)
            {
                await client.EditMessageTextAsync(chatId: chatId,
                    messageId: updateMessage.MessageId, text: $"{string.Join(", ", res.ErrorOutput)}",
                    cancellationToken: cancelToken);
                return;
            }

            long audioFileSize = new FileInfo(res.Data).Length;
            if (audioFileSize >= TelegramFileSizeLimit)
            {
                await client.EditMessageTextAsync(chatId: chatId,
                    messageId: updateMessage.MessageId,
                    text: $"Maximum Audio File size is {Util.FormatFileSie(TelegramFileSizeLimit)}",
                    cancellationToken: cancelToken);
                return;
            }

            string[]? metadata = audioData.Data?.Title.Split("-", 2);
            string? artist = metadata?.Length == 2 ? metadata[0] : null;
            string? title = metadata?.Length == 2 ? metadata[1] : null;

            await using Stream stream = System.IO.File.OpenRead(res.Data);
            await client.SendAudioAsync(
                chatId: chatId,
                audio: InputFile.FromStream(stream),
                cancellationToken: cancelToken,
                title: title,
                performer: artist);

            await client.EditMessageTextAsync(chatId: chatId,
                messageId: updateMessage.MessageId, text: $"Done!", cancellationToken: cancelToken);
            return;

            async void UpdateProgress(DownloadProgress p)
            {
                int progress = (int)Math.Round(p.Progress * 100);
                if (progress == 100 && !startUploading)
                {
                    startUploading = true;
                    await client.EditMessageTextAsync(chatId: chatId,
                        messageId: updateMessage.MessageId, text: $"Uploading audio...",
                        cancellationToken: cancelToken);
                }
            }
        }

        Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancelToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }
}