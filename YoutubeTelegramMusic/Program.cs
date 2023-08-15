using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeTelegramMusic;

const string telegramEnv = "TELEGRAM_BOT_TOKEN";
const string ytDlpEnv = "YOUTUBE_DLP_PATH";
const string ffmpegEnv = "FFMPEG_PATH";
var token = Environment.GetEnvironmentVariable(telegramEnv);

if (token == null) {
    Console.WriteLine($"Expect Telegram token. Set it to environment variable {telegramEnv}");
    return 1;
}
var botClient = new TelegramBotClient(token);
using CancellationTokenSource cts = new ();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new ()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

var options = new OptionSet()
{
    EmbedMetadata = true
};

var ytdl = new YoutubeDL
{
    YoutubeDLPath = Environment.GetEnvironmentVariable(ytDlpEnv),
    FFmpegPath = Environment.GetEnvironmentVariable(ffmpegEnv),
    OutputFolder = "/tmp"
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
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
            cancellationToken: cancellationToken);
        return;
    }

    var updateMessage = await client.SendTextMessageAsync(chatId: chatId, text: "Downloading from youtube...", cancellationToken: cancellationToken);
    var startUploading = false;
    async void UpdateProgress(DownloadProgress p)
    {
        int progress = (int)Math.Round(p.Progress * 100);
        if (progress == 100 && !startUploading)
        {
            startUploading = true;
            await client.EditMessageTextAsync(chatId: chatId,
                messageId: updateMessage.MessageId, text: $"Uploading audio...", cancellationToken: cancellationToken);
        }
    }
    var progress = new Progress<DownloadProgress>(UpdateProgress);

    var res = await ytdl.RunAudioDownload(
        messageText,
        AudioConversionFormat.Mp3,
        progress: progress
    );
    
    var audioData = await ytdl.RunVideoDataFetch(messageText, overrideOptions: options);
    var metadata = audioData.Data.Title.Split("-", 2);
    var artist = metadata.Length == 2 ? metadata[0] : null;
    var title = metadata.Length == 2 ? metadata[1] : null;
    
    await using Stream stream = System.IO.File.OpenRead(res.Data);
    await client.SendAudioAsync(
        chatId: chatId,
        audio: InputFile.FromStream(stream),
        cancellationToken: cancellationToken,
        title: title,
        performer: artist);
        
    await client.EditMessageTextAsync(chatId: chatId, 
        messageId: updateMessage.MessageId, text: $"Done!", cancellationToken: cancellationToken);
}

Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
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

return 0;
