using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YoutubeTelegramMusic.database;

namespace YoutubeTelegramMusic.commands;

public class ChangeLanguage: Command
{
    public override async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancelToken, Locale language = Locale.EN)
    {
        if (update.Message == null) return;
        long chatId = update.Message.Chat.Id;

        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            InlineKeyboardButton.WithCallbackData(text: Localizer.GetValue("English", language), callbackData: "englishLanguage"),
            InlineKeyboardButton.WithCallbackData(text: Localizer.GetValue("Russian", language), callbackData: "russianLanguage"),
        });
        await client.SendTextMessageAsync(
            chatId: chatId,
            text: Localizer.GetValue("SelectLanguage", language),
            cancellationToken: cancelToken,
            replyMarkup: inlineKeyboard
        );
    }
}