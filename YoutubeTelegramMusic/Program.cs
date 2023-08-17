using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace YoutubeTelegramMusic;
public static class Program
{
    public static async Task Main(string[] args)
    {
        var hostBuilder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IHostedService>(new YoutubeBotService());
            });
        await hostBuilder.RunConsoleAsync();
    }
}