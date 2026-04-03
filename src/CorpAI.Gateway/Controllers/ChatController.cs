using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace CorpAI.Gateway.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static List<ChatMessageDto> _chatHistory = new();

    public ChatController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto dto)
    {
        _chatHistory.Add(new ChatMessageDto { Role = "user", Content = dto.Prompt });
        await Task.Delay(500); // Simulate latency
        var response = $"Echo: {dto.Prompt}";
        _chatHistory.Add(new ChatMessageDto { Role = "assistant", Content = response });
        return Ok(_chatHistory);
    }

    [HttpGet("history")]
    public IActionResult GetHistory() => Ok(_chatHistory);
}

public record ChatRequestDto(string Prompt);
public record ChatMessageDto(string Role, string Content);