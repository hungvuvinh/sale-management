using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public interface IChatRepository
{
    Task<ChatSessionEntity?> GetSessionByTokenAsync(string sessionToken);
    Task<ChatSessionEntity?> GetSessionByIdAsync(long sessionId);
    Task<IReadOnlyList<ChatSessionEntity>> GetOpenSessionsAsync(long? storeId);
    Task<ChatSessionEntity> CreateSessionAsync(ChatSessionEntity session);
    Task<IReadOnlyList<ChatMessageEntity>> GetMessagesAsync(long sessionId);
    Task<ChatMessageEntity?> GetMessageByClientIdAsync(long sessionId, string clientMessageId);
    Task<ChatMessageEntity> AddMessageAsync(ChatMessageEntity message);
    Task<bool> TryAssignAgentAsync(long sessionId, long agentUserId);
}