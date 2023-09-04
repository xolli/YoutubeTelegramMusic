using Telegram.Bot;
using Telegram.Bot.Types;

namespace YoutubeTelegramMusic.commands;

public class HelpCommand: Command
{
    public override async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancelToken)
    {
        if (update.Message == null) return;
        long chatId = update.Message.Chat.Id;
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: @"Hi! I can export video from youtube.com. Available commands:
/help — see this message

You can send me a YouTube link and I will export an audio from it. For example: https://youtu.be/dQw4w9WgXcQ",
            cancellationToken: cancelToken,
            disableWebPagePreview: true);
    }
}