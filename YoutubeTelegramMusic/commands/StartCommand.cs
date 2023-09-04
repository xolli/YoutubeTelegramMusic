using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace YoutubeTelegramMusic.commands;

public class StartCommand: Command
{
    public override async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancelToken)
    {
        if (update.Message == null) return;
        long chatId = update.Message.Chat.Id;
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: "Hello! Send me /help, if you want to know how to use me :)",
            cancellationToken: cancelToken);
    }
}