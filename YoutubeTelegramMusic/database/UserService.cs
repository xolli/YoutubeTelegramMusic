namespace YoutubeTelegramMusic.database;

public class UserService
{
    private readonly YtMusicContext _db;

    public UserService(YtMusicContext db)
    {
        _db = db;
    }

    public void CountDownload(Telegram.Bot.Types.User telegramUser)
    {
        var countedUser = (from user in _db.Users where user.UserId == telegramUser.Id select user).FirstOrDefault();
        if (countedUser is not null)
        {
            countedUser.CountDownloads += 1;
        }
        else
        {
            _db.Users.Add(new User
            {
                UserId = telegramUser.Id, CountDownloads = 1, IsAdmin = false, FirstName = telegramUser.FirstName,
                LastName = telegramUser.LastName, Username = telegramUser.Username, Language = Locale.EN
            });
        }

        _db.SaveChanges();
    }

    public bool IsAdmin(long telegramId)
    {
        return (from user in _db.Users where user.IsAdmin && user.UserId == telegramId select user)
            .FirstOrDefault() is not null;
    }

    public Locale GetLocale(long telegramId)
    {
        return (from user in _db.Users where user.UserId == telegramId select user.Language).FirstOrDefault();
    }

    public void InitAdmins(IEnumerable<long> adminsIds)
    {
        var currentAdminsIds = (from user in _db.Users where user.IsAdmin select user.UserId).ToHashSet();
        var updatedAdminIds = adminsIds.ToHashSet();
        var added = updatedAdminIds.Except(currentAdminsIds);
        foreach (long tgId in added)
        {
            CreateOrUpdateAdmin(_db, tgId);
        }

        _db.SaveChanges();
    }

    public void CreateOrUpdateUser(long telegramId, string? username, string firstName, string? lastName)
    {
        var updatedUser = (from user in _db.Users where user.UserId == telegramId select user).FirstOrDefault();
        if (updatedUser is null)
        {
            _db.Users.Add(new User
                { UserId = telegramId, Username = username, FirstName = firstName, LastName = lastName, Language = Locale.EN});
        }
        else
        {
            updatedUser.Username = username;
            updatedUser.FirstName = firstName;
            updatedUser.LastName = lastName;
        }

        _db.SaveChanges();
    }

    public void SetLanguage(long telegramId, Locale language)
    {
        _db.Users.Where(u=> u.UserId  == telegramId).ToList().ForEach(u => u.Language = language);
        _db.SaveChanges();
    }

    private static void CreateOrUpdateAdmin(YtMusicContext db, long tgId)
    {
        var newAdmin = (from user in db.Users where user.UserId == tgId select user).FirstOrDefault();
        if (newAdmin is not null)
        {
            newAdmin.IsAdmin = true;
        }
        else
        {
            db.Users.Add(new User { UserId = tgId, CountDownloads = 0, IsAdmin = true, FirstName = "", Language = Locale.EN});
        }
    }
}