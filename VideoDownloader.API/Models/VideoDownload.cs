using System.ComponentModel.DataAnnotations;

namespace VideoDownloader.API.Models;

public class VideoDownload
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string VideoUrl { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Author { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public string? Quality { get; set; }
    public string Status { get; set; } = "Pending";
    public string Platform { get; set; } = "YouTube";
    public int Progress { get; set; }
    public string? ErrorMessage { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}