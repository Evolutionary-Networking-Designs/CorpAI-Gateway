using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CorpAI.Gateway.Services;

namespace CorpAI.Gateway.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAssetDownloaderService _downloader;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAssetDownloaderService downloader, IWebHostEnvironment env, ILogger<AdminController> logger)
    {
        _downloader = downloader;
        _env = env;
        _logger = logger;
    }

    [HttpPost("download-fonts")]
    public async Task<IActionResult> DownloadFonts([FromBody] FontDownloadRequest request)
    {
        try
        {
            var files = await _downloader.DownloadGoogleFontsAsync(request.FontFamily, request.Weights);
            return Ok(new { Success = true, Message = $"Downloaded {files.Count} font files.", Files = files.Keys.ToList() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download fonts");
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("download-bootstrap")]
    public async Task<IActionResult> DownloadBootstrap()
    {
        await _downloader.DownloadBootstrapAsync();
        return Ok(new { Success = true, Message = "Bootstrap downloaded successfully." });
    }

    [HttpPost("download-jquery")]
    public async Task<IActionResult> DownloadJQuery()
    {
        await _downloader.DownloadJQueryAsync();
        return Ok(new { Success = true, Message = "jQuery downloaded successfully." });
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var env = _env.WebRootPath;
        var hasBootstrap = File.Exists(Path.Combine(env, "lib", "bootstrap", "dist", "css", "bootstrap.min.css"));
        var hasJQuery = File.Exists(Path.Combine(env, "lib", "jquery", "dist", "jquery.min.js"));
        var fontFiles = Directory.Exists(Path.Combine(env, "fonts")) ? Directory.GetFiles(Path.Combine(env, "fonts")).Length : 0;
        return Ok(new { Bootstrap = hasBootstrap, JQuery = hasJQuery, FontCount = fontFiles });
    }
}

public record FontDownloadRequest(string FontFamily, string Weights);