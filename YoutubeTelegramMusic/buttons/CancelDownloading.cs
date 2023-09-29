namespace YoutubeTelegramMusic.buttons;

public class CancelDownloading
{
    private static readonly Dictionary<int, CancellationTokenSource> DownloadsTokens = new();

    public static void AddTokenSource(int messageId, CancellationTokenSource cancellationTokenSource)
    {
        DownloadsTokens.Add(messageId, cancellationTokenSource);
    }

    public static void RemoveToken(int messageId)
    {
        DownloadsTokens.Remove(messageId);
    }

    public static void CancelDownloadingByMessageId(int messageId)
    {
        if (!DownloadsTokens.TryGetValue(messageId, out var tokenSource))
        {
            return;
        }
        
        tokenSource.Cancel(true);
        Console.WriteLine("tokenSource.Cancel(): " + messageId);
        RemoveToken(messageId);
    }
}