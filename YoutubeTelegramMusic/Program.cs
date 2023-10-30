using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YoutubeTelegramMusic.callbacks;
using YoutubeTelegramMusic.commands;
using YoutubeTelegramMusic.database;
using Serilog;

namespace YoutubeTelegramMusic;
public static class Program
{
    private const string LogDirectoryEnv = "YT_LOG_DIRECTORY";
    
    public static void Main() => CreateHostBuilder().Build().Run();
    
    public static IHostBuilder CreateHostBuilder() => Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            SetLogger();
            services.AddDbContext<YtMusicContext>(options =>
            {
                options.UseNpgsql(YtMusicContext.GetConnectionString());
            });
            Log.Information("Application have been started");
            services.AddScoped<UserService>();
            services.AddScoped<CommandFactory>();
            services.AddScoped<CallbackActionFactory>();
            services.AddHostedService<YoutubeBotService>();
        });

    private static void SetLogger()
    {
        string? logDirectory = Environment.GetEnvironmentVariable(LogDirectoryEnv);
        if (logDirectory != null)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(logDirectory, rollingInterval: RollingInterval.Day)
                .CreateLogger();   
        }
        else
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();   
        }
    }
}
