namespace YoutubeTelegramMusic;

public static class Util
{
    private static readonly string[] _addresses = new string[] {"youtu.be", "youtu.be.", "youtube.com", "www.youtube.com", "youtube.com.", "www.youtube.com."};
    private static readonly string[] _schemes = new string[] {"http", "https"};

    public static bool IsYoutubeLink(string text)
    {
        if (!Uri.TryCreate(text, UriKind.Absolute, out var url))
        {
            return false;
        }
        return _addresses.Contains(url.Host) && _schemes.Contains(url.Scheme);
    }
}