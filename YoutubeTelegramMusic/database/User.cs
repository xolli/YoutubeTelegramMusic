using System.ComponentModel.DataAnnotations;

namespace YoutubeTelegramMusic.database;

public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public long UserId { get; set; }
    
    public int CountDownloads { get; set; }

    [Required]
    public bool IsAdmin { get; set;  }
    
    public string? Username { get; set; }
    
    [Required]
    public string FirstName { get; set; }
 
    public string? LastName { get; set; }
    
    public Locale Language { get; set;  }
}