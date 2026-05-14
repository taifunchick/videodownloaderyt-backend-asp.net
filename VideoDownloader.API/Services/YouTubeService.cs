using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace VideoDownloader.API.Services;

public class YouTubeService
{
    private readonly YoutubeClient _client;

    public YouTubeService()
    {
        _client = new YoutubeClient();
    }

    public async Task<object> GetVideoInfoAsync(string url)
    {
        var video = await _client.Videos.GetAsync(url);
        var streamManifest = await _client.Videos.Streams.GetManifestAsync(url);
        
        var qualities = streamManifest.GetVideoStreams()
            .Select(s => new
            {
                quality = s.VideoQuality.Label,
                resolution = $"{s.VideoResolution.Width}x{s.VideoResolution.Height}",
                size = Math.Round(s.Size.MegaBytes, 2),
                format = s.Container.Name
            })
            .Distinct()
            .OrderByDescending(q => q.resolution)
            .ToList();

        return new
        {
            title = video.Title,
            author = video.Author.ChannelTitle,
            duration = video.Duration?.ToString(),
            thumbnailUrl = video.Thumbnails.FirstOrDefault()?.Url ?? "",
            platform = "YouTube",
            qualities
        };
    }

    public async Task<byte[]> DownloadVideoAsync(string url, string quality)
    {
        var streamManifest = await _client.Videos.Streams.GetManifestAsync(url);
        
        var videoStreams = streamManifest.GetVideoStreams()
            .Where(s => s.VideoQuality.Label.Contains(quality) || 
                        $"{s.VideoResolution.Width}x{s.VideoResolution.Height}".Contains(quality))
            .ToList();

        var bestQuality = videoStreams
            .OrderByDescending(s => s.VideoQuality)
            .FirstOrDefault();

        if (bestQuality == null)
            throw new Exception("No suitable quality found");

        using var memoryStream = new MemoryStream();
        var stream = await _client.Videos.Streams.GetAsync(bestQuality);
        await stream.CopyToAsync(memoryStream);
        
        return memoryStream.ToArray();
    }
}