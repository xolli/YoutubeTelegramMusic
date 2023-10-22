using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using YoutubeTelegramMusic.database;

namespace YoutubeTelegramMusic.commands;

public class GetStatistic: Command
{
    private readonly YtMusicContext _db;

    public GetStatistic(YtMusicContext db)
    {
        _db = db;
    }
    
    public override async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancelToken, Locale language = Locale.EN)
    {
        if (update.Message == null) return;
        long chatId = update.Message.Chat.Id;
        var statStringBuilder = new StringBuilder();
        foreach (var user in _db.Users.OrderBy(u => -u.CountDownloads))
        {
            if (user.CountDownloads == 0) continue;
            string name = user.Username is not null ? $"@{user.Username}" : $"{user.FirstName} {user.LastName ?? ""}";
            statStringBuilder.AppendLine($"{name} — {user.CountDownloads}");
        }
        
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: statStringBuilder.ToString(),
            cancellationToken: cancelToken,
            disableWebPagePreview: true);
    }
}