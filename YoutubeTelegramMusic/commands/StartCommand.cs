using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YoutubeTelegramMusic.database;

namespace YoutubeTelegramMusic.commands;

public class StartCommand: Command
{
    public override async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancelToken, Locale language = Locale.EN)
    {
        if (update.Message == null) return;
        long chatId = update.Message.Chat.Id;
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: Localizer.GetValue("Start", language),
            cancellationToken: cancelToken);
    }
}