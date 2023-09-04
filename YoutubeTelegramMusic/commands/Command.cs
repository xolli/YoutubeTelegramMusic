using Telegram.Bot;
using Telegram.Bot.Types;

namespace YoutubeTelegramMusic.commands;

public abstract class Command
{
    public abstract Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancelToken);
}