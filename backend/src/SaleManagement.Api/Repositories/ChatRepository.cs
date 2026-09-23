using Microsoft.EntityFrameworkCore;
using SaleManagement.Api.Data;
using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ChatSessionEntity?> GetSessionByTokenAsync(string sessionToken) =>
        _context.ChatSessions
            .Include(session => session.Store)
            .FirstOrDefaultAsync(session => session.SessionToken == sessionToken);

    public Task<ChatSessionEntity?> GetSessionByIdAsync(long sessionId) =>
        _context.ChatSessions
            .Include(session => session.Store)
            .FirstOrDefaultAsync(session => session.Id == sessionId);

    public async Task<IReadOnlyList<ChatSessionEntity>> GetOpenSessionsAsync(long? storeId)
    {
        return await _context.ChatSessions
            .Include(session => session.Store)
            .Where(session => session.Status == "OPEN" || session.Status == "ASSIGNED")
            .Where(session => !storeId.HasValue || session.StoreId == storeId.Value)
            .OrderBy(session => session.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatSessionEntity> CreateSessionAsync(ChatSessionEntity session)
    {
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<IReadOnlyList<ChatMessageEntity>> GetMessagesAsync(long sessionId)
    {
        return await _context.ChatMessages
            .Where(message => message.SessionId == sessionId)
            .OrderBy(message => message.SentAt)
            .ThenBy(message => message.Id)
            .ToListAsync();
    }

    public Task<ChatMessageEntity?> GetMessageByClientIdAsync(long sessionId, string clientMessageId) =>
        _context.ChatMessages.FirstOrDefaultAsync(message =>
            message.SessionId == sessionId && message.ClientMessageId == clientMessageId);

    public async Task<ChatMessageEntity> AddMessageAsync(ChatMessageEntity message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<bool> TryAssignAgentAsync(long sessionId, long agentUserId)
    {
        var updated = await _context.ChatSessions
            .Where(session => session.Id == sessionId &&
                (session.AssignedAgentUserId == null || session.AssignedAgentUserId == agentUserId) &&
                session.Status != "CLOSED" && session.Status != "RESOLVED")
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(session => session.AssignedAgentUserId, agentUserId)
                .SetProperty(session => session.Status, "ASSIGNED")
                .SetProperty(session => session.UpdatedAt, DateTime.UtcNow));
        return updated > 0;
    }
}