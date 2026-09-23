using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaleManagement.Api.Entities;

[Table("users")]
public class UserEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("email")]
    public string Email { get; set; } = null!;

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [Column("first_name")]
    public string FirstName { get; set; } = null!;

    [Required]
    [Column("last_name")]
    public string LastName { get; set; } = null!;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}

[Table("roles")]
public class RoleEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("code")]
    public string Code { get; set; } = null!;

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }
}

[Table("user_roles")]
public class UserRoleEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; } = null!;

    [Column("role_id")]
    public long RoleId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public RoleEntity Role { get; set; } = null!;

    [Column("store_id")]
    public long? StoreId { get; set; }

    [ForeignKey(nameof(StoreId))]
    public StoreEntity? Store { get; set; }
}
