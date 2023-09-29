using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeTelegramMusic.buttons;
using YoutubeTelegramMusic.commands;

namespace YoutubeTelegramMusic;

public class YoutubeBotService : IHostedService
{
    private const string TelegramEnv = "TELEGRAM_BOT_TOKEN";

    private readonly TelegramBotClient _botClient;

    private readonly YtAudioHandler _ytAudioHandler = new ();

    public YoutubeBotService()
    {
        string? token = Environment.GetEnvironmentVariable(TelegramEnv);
        if (token == null)
        {
            throw new EnvVariablesException($"Expect Telegram token. Set it to environment variable {TelegramEnv}");
        }

        _botClient = new TelegramBotClient(token);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CancellationTokenSource cts = new ();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancelToken)
        {
            if (update.CallbackQuery is { } callback)
            {
                HandleCallback(callback);
                return Task.CompletedTask;
            }
            
            if (update.Message is not { } message)
            {
                return Task.CompletedTask;
            }

            // Only process text messages
            if (message.Text is not { } messageText)
            {
                return Task.CompletedTask;
            }

#pragma warning disable CS4014
            if (Util.IsCommand(messageText))
            {
                HandleCommand(messageText, client, update, cancelToken);
                return Task.CompletedTask;
            }

            _ytAudioHandler.HandleUpdate(client, update, cancelToken);
#pragma warning restore CS4014
            return Task.CompletedTask;
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

        return Task.CompletedTask;
    }

    private static async Task HandleCommand(string command, ITelegramBotClient client, Update update,
        CancellationToken cancelToken)
    {
        await CommandFactory.GetCommandByName(command.Split()[0]).HandleUpdate(client, update, cancelToken);
    }

    private static void HandleCallback(CallbackQuery callback)
    {
        if (callback.Message?.MessageId is not null)
        {
            // FIXME callbackFactory
            CancelDownloading.CancelDownloadingByMessageId(callback.Message.MessageId);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}