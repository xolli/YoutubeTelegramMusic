using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using YoutubeTelegramMusic.callbacks;
using YoutubeTelegramMusic.database;

namespace YoutubeTelegramMusic.buttons;

public class SetRussian: CallbackAction
{
    private readonly UserService _userService;
    
    public SetRussian(UserService userService)
    {
        _userService = userService;
    }

    public void Handle(CallbackQuery callback)
    {
        _userService.SetLanguage(callback.From.Id, Locale.RU);
    }
}