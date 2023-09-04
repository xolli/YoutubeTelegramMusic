using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeTelegramMusic.commands;

namespace YoutubeTelegramMusic;

public class YoutubeBotService : IHostedService
{
    private const string TelegramEnv = "TELEGRAM_BOT_TOKEN";

    private readonly TelegramBotClient _botClient;
    private readonly CancellationTokenSource _cts;

    private readonly YtAudioHandler _ytAudioHandler = new ();

    public YoutubeBotService()
    {
        string? token = Environment.GetEnvironmentVariable(TelegramEnv);
        if (token == null)
        {
            throw new EnvVariablesException($"Expect Telegram token. Set it to environment variable {TelegramEnv}");
        }

        _botClient = new TelegramBotClient(token);
        _cts = new CancellationTokenSource();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token
        );

        return Task.CompletedTask;

        async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancelToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
            {
                return;
            }

            // Only process text messages
            if (message.Text is not { } messageText)
            {
                return;
            }

            if (Util.IsCommand(messageText))
            {
                await HandleCommand(messageText, client, update, cancelToken);
                return;
            }

            await _ytAudioHandler.HandleUpdate(client, update, cancelToken);
        }

        Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancelToken)
        {
            string errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }

    private static async Task HandleCommand(string command, ITelegramBotClient client, Update update,
        CancellationToken cancelToken)
    {
        await CommandFactory.GetCommandByName(command.Split()[0]).HandleUpdate(client, update, cancelToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }
}