using Telegram.Bot.Types;

namespace YoutubeTelegramMusic.callbacks;

public interface CallbackAction
{
    public void Handle(CallbackQuery callback);
}