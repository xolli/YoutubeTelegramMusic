namespace YoutubeTelegramMusic;

public static class Util
{
    private static readonly string[] Addresses = new string[]
        { "youtu.be", "youtu.be.", "youtube.com", "www.youtube.com", "youtube.com.", "www.youtube.com." };

    private static readonly string[] Schemes = new string[] { "http", "https" };

    public static bool IsYoutubeLink(string text)
    {
        if (!Uri.TryCreate(text, UriKind.Absolute, out var url))
        {
            return false;
        }

        return Addresses.Contains(url.Host) && Schemes.Contains(url.Scheme);
    }

    public static string? FormatFileSie(long? bytes)
    {
        if (bytes == null)
        {
            return null;
        }
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double resultSize = (double)bytes;
        while (resultSize >= 1024 && order < sizes.Length - 1)
        {
            order++;
            resultSize /= 1024;
        }

        return $"{resultSize:0.##} {sizes[order]}";
    }
}