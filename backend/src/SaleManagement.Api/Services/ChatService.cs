using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SaleManagement.Api.Services;

public class ChatService
{
    private const int MaxMessageLength = 4000;
    private readonly IChatRepository _chatRepository;
    private readonly IStoreRepository _storeRepository;

    public ChatService(IChatRepository chatRepository, IStoreRepository storeRepository)
    {
        _chatRepository = chatRepository;
        _storeRepository = storeRepository;
    }

    public async Task<ChatSessionDto> CreateSessionAsync(CreateChatSessionRequest request)
    {
        var store = await _storeRepository.GetActiveStoreByIdAsync(request.StoreId)
            ?? throw new KeyNotFoundException("Store không tồn tại hoặc đã ngừng hoạt động.");

        if (!string.IsNullOrWhiteSpace(request.GuestEmail) && !request.GuestEmail.Contains('@'))
            throw new ArgumentException("Guest email không hợp lệ.");

        if (!string.IsNullOrWhiteSpace(request.GuestPostcode))
        {
            var postcode = await _storeRepository.GetPostcodeByCodeAsync(request.GuestPostcode.Trim())
                ?? throw new KeyNotFoundException("Postcode không tồn tại.");
            var isAssigned = await _storeRepository.IsStoreAssignedToPostcodeAsync(postcode.Id, request.StoreId);
            if (!isAssigned)
                throw new ArgumentException("Store không phục vụ postcode đã chọn.");
        }

        var session = await _chatRepository.CreateSessionAsync(new ChatSessionEntity
        {
            SessionToken = Guid.NewGuid().ToString("N"),
            StoreId = store.Id,
            Store = store,
            ParticipantType = "GUEST",
            GuestName = TrimOptional(request.GuestName, 100),
            GuestEmail = TrimOptional(request.GuestEmail, 150),
            GuestPostcode = TrimOptional(request.GuestPostcode, 10)
        });

        return MapSession(session);
    }

    public async Task<IReadOnlyList<ChatSessionDto>> GetOpenSessionsAsync(long? storeId)
    {
        var sessions = await _chatRepository.GetOpenSessionsAsync(storeId);
        return sessions.Select(MapSession).ToArray();
    }

    public async Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(long sessionId, string sessionToken)
    {
        var session = await _chatRepository.GetSessionByTokenAsync(sessionToken)
            ?? throw new KeyNotFoundException("Không tìm thấy phiên chat.");
        if (session.Id != sessionId)
            throw new UnauthorizedAccessException("Session token không khớp với session ID.");
        var messages = await _chatRepository.GetMessagesAsync(sessionId);
        return messages.Select(MapMessage).ToArray();
    }

    public async Task EnsureAgentCanAccessAsync(string sessionToken, long agentUserId, long agentStoreId)
    {
        var session = await _chatRepository.GetSessionByTokenAsync(sessionToken)
            ?? throw new KeyNotFoundException("Không tìm thấy phiên chat.");
        if (session.StoreId != agentStoreId)
            throw new UnauthorizedAccessException("Agent không thuộc store của phiên chat.");
        if (session.AssignedAgentUserId.HasValue && session.AssignedAgentUserId != agentUserId)
            throw new UnauthorizedAccessException("Phiên chat đã được nhận bởi agent khác.");
    }

    public async Task<ChatSessionDto> AssignAgentAsync(long sessionId, long agentUserId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session.Status is "CLOSED" or "RESOLVED")
            throw new InvalidOperationException("Không thể nhận một phiên chat đã đóng.");

        if (!await _chatRepository.TryAssignAgentAsync(sessionId, agentUserId))
            throw new UnauthorizedAccessException("Phiên chat đã được nhận bởi agent khác hoặc đã đóng.");

        var assigned = await GetSessionAsync(sessionId);
        return MapSession(assigned);
    }

    public async Task<(ChatMessageDto Message, bool Created)> AddMessageAsync(
        string sessionToken,
        string clientMessageId,
        string messageText,
        long? senderUserId)
    {
        if (string.IsNullOrWhiteSpace(clientMessageId) || clientMessageId.Length > 100)
            throw new ArgumentException("Client message ID là bắt buộc và tối đa 100 ký tự.");

        if (string.IsNullOrWhiteSpace(messageText) || messageText.Length > MaxMessageLength)
            throw new ArgumentException($"Nội dung tin nhắn phải có từ 1 đến {MaxMessageLength} ký tự.");

        var session = await _chatRepository.GetSessionByTokenAsync(sessionToken)
            ?? throw new KeyNotFoundException("Không tìm thấy phiên chat.");

        if (session.Status is "CLOSED" or "RESOLVED")
            throw new InvalidOperationException("Phiên chat đã đóng.");

        var existing = await _chatRepository.GetMessageByClientIdAsync(session.Id, clientMessageId);
        if (existing != null)
            return (MapMessage(existing), false);

        ChatMessageEntity message;
        try
        {
            message = await _chatRepository.AddMessageAsync(new ChatMessageEntity
            {
                SessionId = session.Id,
                Session = session,
                SenderType = senderUserId.HasValue ? "AGENT" : "GUEST",
                SenderUserId = senderUserId,
                ClientMessageId = clientMessageId,
                MessageText = messageText.Trim()
            });
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: "23505" })
        {
            var retried = await _chatRepository.GetMessageByClientIdAsync(session.Id, clientMessageId)
                ?? throw new InvalidOperationException("Không thể khôi phục message sau xung đột idempotency.", exception);
            return (MapMessage(retried), false);
        }

        return (MapMessage(message), true);
    }

    public async Task<ChatSessionDto> GetSessionAsync(string sessionToken)
    {
        var session = await _chatRepository.GetSessionByTokenAsync(sessionToken)
            ?? throw new KeyNotFoundException("Không tìm thấy phiên chat.");
        return MapSession(session);
    }

    private async Task<ChatSessionEntity> GetSessionAsync(long sessionId) =>
        await _chatRepository.GetSessionByIdAsync(sessionId)
        ?? throw new KeyNotFoundException("Không tìm thấy phiên chat.");

    private static string? TrimOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static ChatSessionDto MapSession(ChatSessionEntity session) => new(
        session.Id,
        session.SessionToken,
        session.StoreId,
        session.Store?.Name ?? string.Empty,
        session.ParticipantType,
        session.Status,
        session.GuestName,
        session.GuestEmail,
        session.GuestPostcode,
        session.AssignedAgentUserId,
        session.CreatedAt);

    private static ChatMessageDto MapMessage(ChatMessageEntity message) => new(
        message.Id,
        message.SessionId,
        message.SenderType,
        message.SenderUserId,
        message.ClientMessageId ?? string.Empty,
        message.MessageText,
        message.SentAt);
}