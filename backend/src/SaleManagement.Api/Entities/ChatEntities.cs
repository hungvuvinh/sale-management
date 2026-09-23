using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaleManagement.Api.Entities;

[Table("chat_sessions")]
public class ChatSessionEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("session_token")]
    public string SessionToken { get; set; } = null!;

    [Column("store_id")]
    public long StoreId { get; set; }

    [ForeignKey(nameof(StoreId))]
    public StoreEntity Store { get; set; } = null!;

    [Column("customer_id")]
    public long? CustomerId { get; set; }

    [Required]
    [Column("participant_type")]
    public string ParticipantType { get; set; } = "GUEST";

    [Column("guest_name")]
    public string? GuestName { get; set; }

    [Column("guest_email")]
    public string? GuestEmail { get; set; }

    [Column("guest_postcode")]
    public string? GuestPostcode { get; set; }

    [Column("assigned_agent_user_id")]
    public long? AssignedAgentUserId { get; set; }

    [Column("status")]
    public string Status { get; set; } = "OPEN";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatMessageEntity> Messages { get; set; } = new List<ChatMessageEntity>();
}

[Table("chat_messages")]
public class ChatMessageEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("session_id")]
    public long SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public ChatSessionEntity Session { get; set; } = null!;

    [Required]
    [Column("sender_type")]
    public string SenderType { get; set; } = null!;

    [Column("sender_user_id")]
    public long? SenderUserId { get; set; }

    [Column("client_message_id")]
    public string? ClientMessageId { get; set; }

    [Required]
    [Column("message_text")]
    public string MessageText { get; set; } = null!;

    [Column("attachment_url")]
    public string? AttachmentUrl { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; }

    [Column("sent_at")]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}