using Telegram.Bot;
using Telegram.Bot.Types;
using YoutubeTelegramMusic.database;

namespace YoutubeTelegramMusic.commands;

public class UnknownCommand: Command
{
    public override async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancelToken, Locale language = Locale.EN)
    {
        if (update.Message == null) return;
        long chatId = update.Message.Chat.Id;
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: Localizer.GetValue("UnknownCommand", language),
            cancellationToken: cancelToken);
    }
}