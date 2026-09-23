using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaleManagement.Api.Entities;

[Table("postcodes")]
public class PostcodeEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("postcode")]
    public string Postcode { get; set; } = null!;

    [Required]
    [Column("suburb_name")]
    public string SuburbName { get; set; } = null!;

    [Required]
    [Column("state")]
    public string State { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }
}

[Table("addresses")]
public class AddressEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("street_address")]
    public string StreetAddress { get; set; } = null!;

    [Column("address_detail")]
    public string? AddressDetail { get; set; }

    [Required]
    [Column("suburb")]
    public string Suburb { get; set; } = null!;

    [Required]
    [Column("city")]
    public string City { get; set; } = null!;

    [Required]
    [Column("state")]
    public string State { get; set; } = null!;

    [Column("postcode_id")]
    public long PostcodeId { get; set; }

    [ForeignKey(nameof(PostcodeId))]
    public PostcodeEntity Postcode { get; set; } = null!;

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("stores")]
public class StoreEntity
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

    [Column("address_id")]
    public long AddressId { get; set; }

    [ForeignKey(nameof(AddressId))]
    public AddressEntity Address { get; set; } = null!;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("warehouses")]
public class WarehouseEntity
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

    [Column("address_id")]
    public long AddressId { get; set; }

    [ForeignKey(nameof(AddressId))]
    public AddressEntity Address { get; set; } = null!;

    [Column("is_store")]
    public bool IsStore { get; set; } = false;

    [Column("total_capacity_cbm")]
    public decimal? TotalCapacityCbm { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("postcode_stores")]
public class PostcodeStoreEntity
{
    [Column("postcode_id")]
    public long PostcodeId { get; set; }

    [ForeignKey(nameof(PostcodeId))]
    public PostcodeEntity Postcode { get; set; } = null!;

    [Column("store_id")]
    public long StoreId { get; set; }

    [ForeignKey(nameof(StoreId))]
    public StoreEntity Store { get; set; } = null!;
}

[Table("store_shipping_rates")]
public class StoreShippingRateEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("store_id")]
    public long StoreId { get; set; }

    [ForeignKey(nameof(StoreId))]
    public StoreEntity Store { get; set; } = null!;

    [Column("min_distance_km")]
    public decimal MinDistanceKm { get; set; }

    [Column("max_distance_km")]
    public decimal MaxDistanceKm { get; set; }

    [Column("shipping_fee_aud")]
    public decimal ShippingFeeAud { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
