using YoutubeTelegramMusic.buttons;
using YoutubeTelegramMusic.database;

namespace YoutubeTelegramMusic.callbacks;

public class CallbackActionFactory
{
    private readonly Dictionary<string, CallbackAction> _callbackActions;

    public CallbackActionFactory(UserService userService)
    {
        _callbackActions = new Dictionary<string, CallbackAction>
        {
            { "cancelDownloading", new CancelDownloadingCallbackAction()},
            { "englishLanguage", new SetEnglish(userService)},
            { "russianLanguage", new SetRussian(userService)}
        };
    }

    public CallbackAction? GetCallbackActionByName(string name)
    {
        return !_callbackActions.TryGetValue(name, out var resultCommand) ? null : resultCommand;
    }
}