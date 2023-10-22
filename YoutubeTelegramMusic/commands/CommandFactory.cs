using YoutubeTelegramMusic.database;

namespace YoutubeTelegramMusic.commands;

public class CommandFactory
{
    private readonly Dictionary<string, Command> _userCommands;
    private readonly Dictionary<string, Command> _adminCommands;

    private readonly Command _defaultCommand;

    public CommandFactory(YtMusicContext db)
    {
        _userCommands = new Dictionary<string, Command>
        {
            { "/start", new StartCommand() },
            { "/help", new HelpCommand() },
            { "/language", new ChangeLanguage() }
        };
        _adminCommands = new Dictionary<string, Command>
        {
            { "/stat", new GetStatistic(db) }
        };
        _defaultCommand = new UnknownCommand();
    }

    public Command GetUserCommandByName(string commandName)
    {
        return !_userCommands.TryGetValue(commandName, out var resultCommand) ? _defaultCommand : resultCommand;
    }

    public Command GetAdminCommand(string commandName)
    {
        return !_adminCommands.TryGetValue(commandName, out var resultCommand)
            ? GetUserCommandByName(commandName)
            : resultCommand;
    }
}