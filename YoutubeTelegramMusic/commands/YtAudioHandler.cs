using Telegram.Bot;
using Telegram.Bot.Types;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace YoutubeTelegramMusic.commands;

public class YtAudioHandler : Command
{
    private const string YtDlpEnv = "YOUTUBE_DLP_PATH";
    private const string FfmpegEnv = "FFMPEG_PATH";
    private static readonly string OutputFolder = Path.GetTempPath();
    private const long TelegramFileSizeLimit = 50 * 1024 * 1024;

    private static readonly OptionSet Options = new()
    {
        EmbedMetadata = true,
        MaxFilesize = "50M",
        NoPlaylist = true,
        MaxDownloads = 2
    };

    private readonly YoutubeDL _ytdl;

    public YtAudioHandler()
    {
        string? ffmpegPath = Environment.GetEnvironmentVariable(FfmpegEnv);
        string? youtubeDlPath = Environment.GetEnvironmentVariable(YtDlpEnv);
        if (ffmpegPath == null)
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
            FFmpegPath = ffmpegPath,
            OutputFolder = OutputFolder
        };
    }

    public override async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancelToken)
    {
        if (update.Message == null) return;
        long chatId = update.Message.Chat.Id;
        
        string messageText = update.Message.Text ?? "";

        if (!Util.IsYoutubeLink(messageText))
        {
            await client.SendTextMessageAsync(
                chatId: chatId,
                text: "It is not a valid YouTube link",
                cancellationToken: cancelToken);
            return;
        }

        if (Util.IsPlaylist(messageText))
        {
            await client.SendTextMessageAsync(
                chatId: chatId,
                text: "It is a playlist",
                cancellationToken: cancelToken);
            return;
        }

        var updateMessage = await client.SendTextMessageAsync(chatId: chatId, text: "Downloading from youtube...",
            cancellationToken: cancelToken);
        var startUploading = false;

        var progress = new Progress<DownloadProgress>(UpdateProgress);

        var audioData = await _ytdl.RunVideoDataFetch(messageText, overrideOptions: Options, ct: cancelToken);
        if (!audioData.Success)
        {
            await ReportError(audioData.ErrorOutput, client, chatId, updateMessage, cancelToken);
            return;
        }

        var res = await _ytdl.RunAudioDownload(
            messageText,
            AudioConversionFormat.Mp3,
            progress: progress,
            ct: cancelToken,
            overrideOptions: Options);

        if (!res.Success)
        {
            await ReportError(res.ErrorOutput, client, chatId, updateMessage, cancelToken);
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
            int roundedProgress = (int)Math.Round(p.Progress * 100);
            if (roundedProgress != 100 || startUploading)
            {
                return;
            }

            startUploading = true;
            await client.EditMessageTextAsync(chatId: chatId,
                messageId: updateMessage.MessageId, text: $"Uploading audio...",
                cancellationToken: cancelToken);
        }
    }

    private static async Task ReportError(string[] errorOutput, ITelegramBotClient client, long chatId,
        Message updateMessage, CancellationToken cancelToken)
    {
        string errorMessage = string.Join(", ", errorOutput);
        errorMessage = errorMessage.Trim().Length > 0 ? errorMessage : "Undefined error";
        await client.EditMessageTextAsync(chatId: chatId,
            messageId: updateMessage.MessageId, text: $"{errorMessage}",
            cancellationToken: cancelToken);
    }
}