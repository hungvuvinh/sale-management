namespace SaleManagement.Api.Models;

public record CreateChatSessionRequest(
    long StoreId,
    string? GuestName,
    string? GuestEmail,
    string? GuestPostcode
);

public record ChatSessionDto(
    long Id,
    string SessionToken,
    long StoreId,
    string StoreName,
    string ParticipantType,
    string Status,
    string? GuestName,
    string? GuestEmail,
    string? GuestPostcode,
    long? AssignedAgentUserId,
    DateTime CreatedAt
);

public record ChatMessageDto(
    long Id,
    long SessionId,
    string SenderType,
    long? SenderUserId,
    string ClientMessageId,
    string MessageText,
    DateTime SentAt
);