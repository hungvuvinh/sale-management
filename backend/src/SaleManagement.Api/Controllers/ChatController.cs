using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaleManagement.Api.Models;
using SaleManagement.Api.Services;

namespace SaleManagement.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("session")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateSession([FromBody] CreateChatSessionRequest request)
    {
        try
        {
            var session = await _chatService.CreateSessionAsync(request);
            return CreatedAtAction(nameof(GetMessages), new { id = session.Id }, new { success = true, data = session });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("sessions")]
    [Authorize(Roles = "CS_AGENT")]
    public async Task<IActionResult> GetOpenSessions()
    {
        var storeIdValue = User.FindFirstValue("store_id");
        if (!long.TryParse(storeIdValue, out var storeId))
            return Forbid();

        var sessions = await _chatService.GetOpenSessionsAsync(storeId);
        return Ok(new { success = true, data = sessions });
    }

    [HttpGet("sessions/{id:long}/messages")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMessages(
        long id,
        [FromHeader(Name = "X-Chat-Session-Token")] string? sessionToken)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
            return Unauthorized(new { success = false, message = "Session token is required." });

        try
        {
            var messages = await _chatService.GetMessagesAsync(id, sessionToken);
            return Ok(new { success = true, data = messages });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
}