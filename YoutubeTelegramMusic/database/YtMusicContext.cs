using Microsoft.EntityFrameworkCore;

namespace YoutubeTelegramMusic.database;

public class YtMusicContext: DbContext
{
    private const string UsernameEnv = "YT_PS_USERNAME";
    private const string PasswordEnv = "YT_PS_PASSWORD";
    private const string HostnameEnv = "YT_PS_HOSTNAME";
    private const string DatabaseEnv = "YT_PS_DATABASE";
    
    public YtMusicContext(DbContextOptions<YtMusicContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(GetConnectionString());
    }

    public static string GetConnectionString()
    {
        string? username = Environment.GetEnvironmentVariable(UsernameEnv);
        if (username == null)
        {
            username = "postgres";
        }
        string? password = Environment.GetEnvironmentVariable(PasswordEnv);
        if (password == null)
        {
            password = "";
        }
        string? hostname = Environment.GetEnvironmentVariable(HostnameEnv);
        if (hostname == null)
        {
            // logging
            hostname = "localhost";
        }
        string? database = Environment.GetEnvironmentVariable(DatabaseEnv);
        if (database == null)
        {
            database = username;
        }
        return $"User Id={username};Password={password};Host={hostname};Database={database};";
    }

    public DbSet<User> Users { get; set; }
}