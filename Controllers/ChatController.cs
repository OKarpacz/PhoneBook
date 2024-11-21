using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PhoneBook.Services;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("welcome")]
    public IActionResult Welcome()
    {
        return Ok("Hello! I'm your Phone Book assistant. I can add, find, edit, and delete contacts. Just tell me what you need!");
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskLLM([FromBody] string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
        {
            return BadRequest("Prompt cannot be empty.");
        }

        return await _chatService.ProcessPrompt(prompt);
    }
}