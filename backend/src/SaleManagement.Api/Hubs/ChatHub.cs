using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SaleManagement.Api.Options;
using SaleManagement.Api.Services;
using StackExchange.Redis;

namespace SaleManagement.Api.Hubs;

public class ChatHub : Hub
{
    private readonly ChatService _chatService;
    private readonly IRateLimitStore _rateLimitStore;
    private readonly RateLimitOptions _rateLimitOptions;

    public ChatHub(
        ChatService chatService,
        IRateLimitStore rateLimitStore,
        IOptions<RateLimitOptions> rateLimitOptions)
    {
        _chatService = chatService;
        _rateLimitStore = rateLimitStore;
        _rateLimitOptions = rateLimitOptions.Value;
    }

    public async Task join_session(string sessionToken)
    {
        await EnsureEventAllowedAsync(sessionToken);
        var session = await _chatService.GetSessionAsync(sessionToken);
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(session.Id));
    }

    [Authorize(Roles = "CS_AGENT")]
    public async Task agent_join(long sessionId, string sessionToken)
    {
        await EnsureEventAllowedAsync(sessionToken);
        var session = await _chatService.GetSessionAsync(sessionToken);
        if (session.Id != sessionId)
            throw new HubException("Session token không khớp với session ID.");

        var agentId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(agentId, out var parsedAgentId))
            throw new HubException("Không xác định được agent.");

        var storeId = Context.User?.FindFirstValue("store_id");
        if (!long.TryParse(storeId, out var parsedStoreId))
            throw new HubException("Agent chưa được gán store.");
        await _chatService.EnsureAgentCanAccessAsync(sessionToken, parsedAgentId, parsedStoreId);

        await _chatService.AssignAgentAsync(sessionId, parsedAgentId);
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(sessionId));
    }

    public async Task send_message(string sessionToken, string clientMessageId, string messageText)
    {
        await EnsureEventAllowedAsync(sessionToken);
        long? senderUserId = null;
        if (Context.User?.IsInRole("CS_AGENT") == true &&
            long.TryParse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var agentId))
        {
            senderUserId = agentId;
            var storeId = Context.User.FindFirstValue("store_id");
            if (!long.TryParse(storeId, out var parsedStoreId))
                throw new HubException("Agent chưa được gán store.");
            await _chatService.EnsureAgentCanAccessAsync(sessionToken, agentId, parsedStoreId);
        }

        var result = await _chatService.AddMessageAsync(
            sessionToken,
            clientMessageId,
            messageText,
            senderUserId);

        if (result.Created)
        {
            await Clients.Group(GroupName(result.Message.SessionId))
                .SendAsync("receive_message", result.Message);
        }
    }

    public async Task typing(string sessionToken, bool isTyping)
    {
        await EnsureEventAllowedAsync(sessionToken);
        var session = await _chatService.GetSessionAsync(sessionToken);
        await Clients.Group(GroupName(session.Id))
            .SendAsync("typing", new { sessionId = session.Id, isTyping });
    }

    private static string GroupName(long sessionId) => $"chat-session:{sessionId}";

    private async Task EnsureEventAllowedAsync(string sessionToken)
    {
        if (string.IsNullOrWhiteSpace(sessionToken) || sessionToken.Length > 100)
            throw new HubException("Session token không hợp lệ.");

        var identity = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "guest";
        var capacity = Context.User?.IsInRole("CS_AGENT") == true
            ? _rateLimitOptions.StaffRequestsPerMinute
            : _rateLimitOptions.PublicRequestsPerMinute;
        try
        {
            var decision = await _rateLimitStore.ConsumeAsync(
                $"rate-limit:chat:{identity}:{HashToken(sessionToken)}",
                capacity,
                TimeSpan.FromMinutes(1),
                Context.ConnectionAborted);
            if (!decision.IsAllowed)
                throw new HubException($"Quá số lượng tin nhắn cho phép. Thử lại sau {decision.RetryAfterSeconds} giây.");
        }
        catch (Exception exception) when (exception is RedisConnectionException or RedisTimeoutException)
        {
            throw new HubException("Chat tạm thời không khả dụng.", exception);
        }
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}