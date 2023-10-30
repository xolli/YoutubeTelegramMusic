using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeTelegramMusic.buttons;
using YoutubeTelegramMusic.database;
using File = System.IO.File;

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
            OutputFolder = OutputFolder,
            OutputFileTemplate = "[%(id)s]-%(epoch)s.%(ext)s" // FIXME error of 2 users try to download the same video at the same second
        };
    }

    public override async Task<bool> HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancelToken, Locale language = Locale.EN)
    {
        if (update.Message == null) return false;
        long chatId = update.Message.Chat.Id;
        
        string messageText = update.Message.Text ?? "";

        if (!Util.IsYoutubeLink(messageText))
        {
            await client.SendTextMessageAsync(
                chatId: chatId,
                text: Localizer.GetValue("NotValidYoutubeLink", language),
                cancellationToken: cancelToken);
            return false;
        }

        if (Util.IsPlaylist(messageText))
        {
            await client.SendTextMessageAsync(
                chatId: chatId,
                text: Localizer.GetValue("Playlist", language),
                cancellationToken: cancelToken);
            return false;
        }
        
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            InlineKeyboardButton.WithCallbackData(text: Localizer.GetValue("CancelDownloading", language), callbackData: "cancelDownloading"),
        });

        var updateMessage = await client.SendTextMessageAsync(
            chatId: chatId,
            text: Localizer.GetValue("Downloading", language),
            cancellationToken: cancelToken,
            replyMarkup: inlineKeyboard
            );

        var downloadingCancelTokenSource = new CancellationTokenSource();
        var downloadingCancelToken = downloadingCancelTokenSource.Token;
        downloadingCancelToken.Register(async () =>
        {
            await client.EditMessageTextAsync(chatId: chatId,
                messageId: updateMessage.MessageId, text: Localizer.GetValue("CanceledDownloading", language), cancellationToken: cancelToken);
        });
        CancelDownloadingCallbackAction.AddTokenSource(updateMessage.MessageId, downloadingCancelTokenSource);
        var startUploading = false;

        var progress = new Progress<DownloadProgress>(UpdateProgress);

        var audioData = await _ytdl.RunVideoDataFetch(messageText, overrideOptions: Options, ct: cancelToken);
        if (!audioData.Success)
        {
            await ReportError(audioData.ErrorOutput, messageText, client, chatId, updateMessage, language, cancelToken);
            return false;
        }

        var res = await _ytdl.RunAudioDownload(
            messageText,
            AudioConversionFormat.Mp3,
            progress: progress,
            ct: downloadingCancelToken,
            overrideOptions: Options);
        
        if (!res.Success)
        {
            await ReportError(res.ErrorOutput, messageText, client, chatId, updateMessage, language, cancelToken);
            return false;
        }

        long audioFileSize = new FileInfo(res.Data).Length;
        if (audioFileSize >= TelegramFileSizeLimit)
        {
            await client.EditMessageTextAsync(chatId: chatId,
                messageId: updateMessage.MessageId,
                text: String.Format(Localizer.GetValue("MaxAudiSize", language), Util.FormatFileSie(TelegramFileSizeLimit)),
                cancellationToken: cancelToken);
            return false;
        }

        string[]? metadata = audioData.Data?.Title.Split("-", 2);
        string? artist = metadata?.Length == 2 ? metadata[0] : null;
        string? title = metadata?.Length == 2 ? metadata[1] : null;

        await using Stream stream = File.OpenRead(res.Data);
        await client.SendAudioAsync(
            chatId: chatId,
            audio: InputFile.FromStream(stream),
            cancellationToken: downloadingCancelToken,
            title: title,
            performer: artist);

        await client.EditMessageTextAsync(chatId: chatId,
            messageId: updateMessage.MessageId, text: Localizer.GetValue("Done", language), cancellationToken: cancelToken);
        File.Delete(res.Data);
        
        return true;

        async void UpdateProgress(DownloadProgress p)
        {
            var roundedProgress = (int)Math.Round(p.Progress * 100);
            if (roundedProgress != 100 || startUploading)
            {
                return;
            }

            startUploading = true;
            await client.EditMessageTextAsync(chatId: chatId,
                messageId: updateMessage.MessageId, text: Localizer.GetValue("UploadingAudio", language),
                cancellationToken: cancelToken,
                replyMarkup: inlineKeyboard
                );
        }
    }

    private static async Task ReportError(string[] errorOutput, string messageText, ITelegramBotClient client, long chatId,
        Message updateMessage, Locale language, CancellationToken cancelToken)
    {
        string errorMessage = string.Join(", ", errorOutput);
        Log.Error($"{errorMessage}, message: {messageText}");
        errorMessage = errorMessage.Trim().Length > 0 ? errorMessage : Localizer.GetValue("UndefinedError", language);
        await client.EditMessageTextAsync(chatId: chatId,
            messageId: updateMessage.MessageId, text: $"{errorMessage}",
            cancellationToken: cancelToken);
    }
}