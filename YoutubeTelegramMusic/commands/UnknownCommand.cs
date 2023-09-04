using Telegram.Bot;
using Telegram.Bot.Types;

namespace YoutubeTelegramMusic.commands;

public class UnknownCommand: Command
{
    public override async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancelToken)
    {
        if (update.Message == null) return;
        long chatId = update.Message.Chat.Id;
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Unknown command =(",
            cancellationToken: cancelToken);
    }
}