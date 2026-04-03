using System.Security.Cryptography;
using System.Text;

namespace CorpAI.Gateway.Services;

public interface IThemeGeneratorService
{
    string GetCssFileName();
    string GetCssContent();
}

public class ThemeGeneratorService : IThemeGeneratorService
{
    private readonly string _cssContent;
    private readonly string _fileName;

    public ThemeGeneratorService()
    {
        var fontImport = "@import url('/fonts/roboto.css');";
        var styles = @"
            :root { --primary-color: #0d6efd; --secondary-color: #6c757d; --chat-bg-user: #0d6efd; --chat-bg-assistant: #6c757d; --font-family: 'Roboto', sans-serif; }
            body { font-family: var(--font-family); background-color: #f8f9fa; }
            .chat-bubble-user { background-color: var(--chat-bg-user); color: white; border-radius: 12px 12px 0 12px; }
            .chat-bubble-assistant { background-color: var(--chat-bg-assistant); color: white; border-radius: 12px 12px 12px 0; }
            .navbar-custom { background: linear-gradient(90deg, #1a1a2e 0%, #16213e 100%); }
            .btn-primary-custom { background-color: var(--primary-color); border: none; transition: transform 0.2s; }
            .btn-primary-custom:hover { transform: scale(1.05); }
        ";
        _cssContent = fontImport + styles;

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(_cssContent));
        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 8);
        _fileName = $"theme.{hashString}.css";
    }

    public string GetCssFileName() => _fileName;
    public string GetCssContent() => _cssContent;
}