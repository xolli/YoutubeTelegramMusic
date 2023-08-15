namespace YoutubeTelegramMusic;

public static class Util
{
    private static readonly string[] Addresses = new string[] {"youtu.be", "youtu.be.", "youtube.com", "www.youtube.com", "youtube.com.", "www.youtube.com."};
    private static readonly string[] Schemes = new string[] {"http", "https"};

    public static bool IsYoutubeLink(string text)
    {
        if (!Uri.TryCreate(text, UriKind.Absolute, out var url))
        {
            return false;
        }
        return Addresses.Contains(url.Host) && Schemes.Contains(url.Scheme);
    }
}