using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeTelegramMusic.callbacks;
using YoutubeTelegramMusic.commands;
using YoutubeTelegramMusic.database;

namespace YoutubeTelegramMusic;

public class YoutubeBotService : IHostedService
{
    private const string TelegramEnv = "TELEGRAM_BOT_TOKEN";

    private const string AdminsListIdsEnv = "ADMIN_TG_IDS";

    private readonly TelegramBotClient _botClient;

    private readonly YtAudioHandler _ytAudioHandler = new();

    private readonly IServiceScopeFactory _scopeFactory;

    public YoutubeBotService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        using (var scope = scopeFactory.CreateScope())
        {
            var ytMusicDbContext = scope.ServiceProvider.GetRequiredService<YtMusicContext>();
            ytMusicDbContext.Database.Migrate();
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            string? adminIdList = Environment.GetEnvironmentVariable(AdminsListIdsEnv);
            if (adminIdList is not null)
            {
                userService.InitAdmins(adminIdList.Split(",").Select(long.Parse).ToList());
            }
        }

        string? token = Environment.GetEnvironmentVariable(TelegramEnv);
        if (token == null)
        {
            throw new EnvVariablesException($"Expect Telegram token. Set it to environment variable {TelegramEnv}");
        }

        _botClient = new TelegramBotClient(token);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CancellationTokenSource cts = new();

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
            using var userServiceScope = _scopeFactory.CreateScope();
            var userService = userServiceScope.ServiceProvider.GetRequiredService<UserService>();
            if (update.Message?.From is not null)
            {
                userService.CreateOrUpdateUser(update.Message.From.Id, update.Message.From.Username,
                    update.Message.From.FirstName, update.Message.From.LastName);
            }

            if (update.CallbackQuery is { } callback)
            {
                HandleCallback(client, callback);
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

            if (Util.IsCommand(messageText))
            {
                HandleCommand(messageText, client, update, cancelToken);
                return Task.CompletedTask;
            }

            var userLanguage = update.Message.From?.Id is not null
                ? userService.GetLocale(update.Message.From.Id)
                : Locale.EN;
            _ytAudioHandler.HandleUpdate(client, update, cancelToken, userLanguage).ContinueWith((task) =>
            {
                using var userServiceScopeCountDownload = _scopeFactory.CreateScope();
                var userServiceCountDownload = userServiceScopeCountDownload.ServiceProvider.GetRequiredService<UserService>();
                if (!task.Result || update.Message?.From is null)
                {
                    Log.Information("User {A} has tried to download some youtube video and it wasn't successful", update.Message?.From?.Username != null ? $"@{update.Message.From.Username}" : update.Message.From?.FirstName);
                    return;
                }
                Log.Information("User {A} has downloaded some youtube video", update.Message.From?.Username != null ? $"@{update.Message.From.Username}" : update.Message.From?.FirstName);
                userServiceCountDownload.CountDownload(update.Message.From);
            }, cancelToken);
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

    private async Task HandleCommand(string command, ITelegramBotClient client, Update update,
        CancellationToken cancelToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<CommandFactory>();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        string commandName = command.Split()[0];
        if (update.Message?.From is not null && userService.IsAdmin(update.Message.From.Id))
        {
            await factory.GetAdminCommand(commandName).HandleUpdate(client, update, cancelToken,
                userService.GetLocale(update.Message.From.Id));
        }
        else if (update.Message?.From is not null)
        {
            await factory.GetUserCommandByName(commandName).HandleUpdate(client, update, cancelToken,
                userService.GetLocale(update.Message.From.Id));
        }
        else
        {
            await factory.GetUserCommandByName(commandName).HandleUpdate(client, update, cancelToken);
        }
    }

    private void HandleCallback(ITelegramBotClient client, CallbackQuery callback)
    {
        if (callback.Data is null || callback.Message is null) return;
        using var scope = _scopeFactory.CreateScope();
        var callbackFactory = scope.ServiceProvider.GetRequiredService<CallbackActionFactory>();
        var callbackAction = callbackFactory.GetCallbackActionByName(callback.Data);
        callbackAction?.Handle(callback);
        client.MakeRequestAsync(new AnswerCallbackQueryRequest(callback.Id));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}