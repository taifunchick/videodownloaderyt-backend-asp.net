using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using VideoDownloader.API.Data;
using VideoDownloader.API.Models;
using VideoDownloader.API.Services;

namespace VideoDownloader.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly YouTubeService _youTubeService;
    private readonly IConfiguration _configuration;

    public VideoController(AppDbContext context, YouTubeService youTubeService, IConfiguration configuration)
    {
        _context = context;
        _youTubeService = youTubeService;
        _configuration = configuration;
    }

    [HttpPost("info")]
    public async Task<IActionResult> GetVideoInfo([FromBody] VideoInfoRequest request)
    {
        try
        {
            var info = await _youTubeService.GetVideoInfoAsync(request.Url);
            return Ok(info);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("download")]
    public async Task<IActionResult> DownloadVideo([FromBody] DownloadRequest request)
    {
        try
        {
            var userId = GetUserId();
            var videoData = await _youTubeService.DownloadVideoAsync(request.Url, request.Quality ?? "720p");
            
            var download = new VideoDownload
            {
                VideoUrl = request.Url,
                Title = request.Title,
                Quality = request.Quality,
                Platform = "YouTube",
                UserId = userId,
                Status = "Completed",
                FileSize = videoData.Length,
                CompletedAt = DateTime.UtcNow
            };

            _context.VideoDownloads.Add(download);
            await _context.SaveChangesAsync();

            return Ok(new { downloadId = download.Id, message = "Download completed" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("downloads")]
    public async Task<IActionResult> GetUserDownloads()
    {
        var userId = GetUserId();
        var downloads = await _context.VideoDownloads
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new
            {
                v.Id,
                v.Title,
                v.Quality,
                v.Status,
                v.Progress,
                v.FileSize,
                v.CreatedAt,
                v.CompletedAt,
                v.Platform
            })
            .ToListAsync();

        return Ok(downloads);
    }

    [Authorize]
    [HttpDelete("download/{id}")]
    public async Task<IActionResult> DeleteDownload(int id)
    {
        var userId = GetUserId();
        var download = await _context.VideoDownloads
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

        if (download == null)
            return NotFound();

        _context.VideoDownloads.Remove(download);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Download deleted" });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { error = "Email already exists" });

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return Ok(new { token, user = new { user.Id, user.Email, user.Username } });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid email or password" });

        var token = GenerateJwtToken(user);
        return Ok(new { token, user = new { user.Id, user.Email, user.Username } });
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "default-secret-key-32-chars-long!!!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class VideoInfoRequest { public string Url { get; set; } = string.Empty; }
public class DownloadRequest { public string Url { get; set; } = string.Empty; public string Title { get; set; } = string.Empty; public string? Quality { get; set; } }
public class RegisterRequest { public string Email { get; set; } = string.Empty; public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class LoginRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }