using Telegram.Bot.Types;
using YoutubeTelegramMusic.callbacks;

namespace YoutubeTelegramMusic.buttons;

public class CancelDownloadingCallbackAction : CallbackAction
{
    private static readonly Dictionary<int, CancellationTokenSource> DownloadsTokens = new();

    public static void AddTokenSource(int messageId, CancellationTokenSource cancellationTokenSource)
    {
        DownloadsTokens.Add(messageId, cancellationTokenSource);
    }

    public void Handle(CallbackQuery callback)
    {
        if (callback.Message is null) return;
        if (!DownloadsTokens.TryGetValue(callback.Message.MessageId, out var tokenSource))
        {
            return;
        }
        
        tokenSource.Cancel(true);
        RemoveToken(callback.Message.MessageId);
    }

    private static void RemoveToken(int messageId)
    {
        DownloadsTokens.Remove(messageId);
    }
}