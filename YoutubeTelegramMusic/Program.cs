using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YoutubeTelegramMusic.callbacks;
using YoutubeTelegramMusic.commands;
using YoutubeTelegramMusic.database;

namespace YoutubeTelegramMusic;
public static class Program
{
    public static void Main() => CreateHostBuilder().Build().Run();
    
    public static IHostBuilder CreateHostBuilder() => Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            services.AddDbContext<YtMusicContext>(options =>
            {
                options.UseNpgsql(YtMusicContext.GetConnectionString());
            });
            services.AddScoped<UserService>();
            services.AddScoped<CommandFactory>();
            services.AddScoped<CallbackActionFactory>();
            services.AddHostedService<YoutubeBotService>();
        });
}
