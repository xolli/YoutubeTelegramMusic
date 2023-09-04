namespace YoutubeTelegramMusic.commands;

public static class CommandFactory
{
    private static readonly Dictionary<string, Command> Commands;

    private static readonly Command DefaultCommand;

    static CommandFactory()
    {
        Commands = new Dictionary<string, Command>()
        {
            { "/start", new StartCommand() },
            { "/help", new HelpCommand() },
        };
        DefaultCommand = new UnknownCommand();
    }

    public static Command GetCommandByName(string commandName)
    {
        return !Commands.TryGetValue(commandName, out var resultCommand) ? DefaultCommand : resultCommand;
    }
}
