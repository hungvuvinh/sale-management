using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaleManagement.Api.Entities;

[Table("delivery_service_rates")]
public class DeliveryServiceRateEntity
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

    [Column("fee_type")]
    public string FeeType { get; set; } = "FLAT"; // PER_FLOOR, FLAT, PER_UNIT

    [Column("fee_aud")]
    public decimal FeeAud { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("carriers")]
public class CarrierEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Required]
    [Column("code")]
    public string Code { get; set; } = null!;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("is_internal")]
    public bool IsInternal { get; set; }

    [Column("rate_card_json", TypeName = "jsonb")]
    public string? RateCardJson { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("drivers")]
public class DriverEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("carrier_id")]
    public long CarrierId { get; set; }

    [ForeignKey(nameof(CarrierId))]
    public CarrierEntity Carrier { get; set; } = null!;

    [Column("user_id")]
    public long? UserId { get; set; }

    [Required]
    [Column("full_name")]
    public string FullName { get; set; } = null!;

    [Required]
    [Column("phone")]
    public string Phone { get; set; } = null!;

    [Column("license_number")]
    public string? LicenseNumber { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("delivery_bookings")]
public class DeliveryBookingEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("order_id")]
    public long OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public OrderEntity Order { get; set; } = null!;

    [Column("delivery_address_id")]
    public long DeliveryAddressId { get; set; }

    [ForeignKey(nameof(DeliveryAddressId))]
    public AddressEntity DeliveryAddress { get; set; } = null!;

    [Column("scheduled_date")]
    public DateTime ScheduledDate { get; set; }

    [Column("time_slot")]
    public string TimeSlot { get; set; } = "FLEXIBLE"; // MORNING, AFTERNOON, EVENING, FLEXIBLE

    [Column("is_assembling")]
    public bool IsAssembling { get; set; }

    [Column("is_upstairs")]
    public bool IsUpstairs { get; set; }

    [Column("stairs_floor_count")]
    public int StairsFloorCount { get; set; }

    [Column("special_notes")]
    public string? SpecialNotes { get; set; }

    [Column("surcharge_aud")]
    public decimal SurchargeAud { get; set; }

    [Column("status")]
    public string Status { get; set; } = "BOOKED"; // BOOKED, ASSIGNED, IN_TRANSIT, DELIVERED, COMEBACK, CANCELLED

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<DeliveryBookingServiceEntity> Services { get; set; } = new();
}

[Table("delivery_booking_services")]
public class DeliveryBookingServiceEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("booking_id")]
    public long BookingId { get; set; }

    [ForeignKey(nameof(BookingId))]
    public DeliveryBookingEntity Booking { get; set; } = null!;

    [Column("service_rate_id")]
    public long ServiceRateId { get; set; }

    [ForeignKey(nameof(ServiceRateId))]
    public DeliveryServiceRateEntity ServiceRate { get; set; } = null!;

    [Column("quantity")]
    public int Quantity { get; set; } = 1;

    [Column("unit_fee_aud")]
    public decimal UnitFeeAud { get; set; }

    [Column("line_surcharge_aud")]
    public decimal LineSurchargeAud { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("delivery_routes")]
public class DeliveryRouteEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("route_code")]
    public string RouteCode { get; set; } = null!;

    [Column("carrier_id")]
    public long CarrierId { get; set; }

    [ForeignKey(nameof(CarrierId))]
    public CarrierEntity Carrier { get; set; } = null!;

    [Column("driver_id")]
    public long? DriverId { get; set; }

    [ForeignKey(nameof(DriverId))]
    public DriverEntity? Driver { get; set; }

    [Column("origin_warehouse_id")]
    public long OriginWarehouseId { get; set; }

    [ForeignKey(nameof(OriginWarehouseId))]
    public WarehouseEntity OriginWarehouse { get; set; } = null!;

    [Column("delivery_date")]
    public DateTime DeliveryDate { get; set; }

    [Column("total_distance_km")]
    public decimal TotalDistanceKm { get; set; }

    [Column("total_duration_mins")]
    public int TotalDurationMins { get; set; }

    [Column("status")]
    public string Status { get; set; } = "PLANNED"; // PLANNED, IN_PROGRESS, COMPLETED, CANCELLED

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<DeliveryStopEntity> Stops { get; set; } = new();
}

[Table("delivery_stops")]
public class DeliveryStopEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("route_id")]
    public long RouteId { get; set; }

    [ForeignKey(nameof(RouteId))]
    public DeliveryRouteEntity Route { get; set; } = null!;

    [Column("booking_id")]
    public long BookingId { get; set; }

    [ForeignKey(nameof(BookingId))]
    public DeliveryBookingEntity Booking { get; set; } = null!;

    [Column("sequence_order")]
    public int SequenceOrder { get; set; }

    [Column("estimated_arrival")]
    public DateTime? EstimatedArrival { get; set; }

    [Column("actual_arrival")]
    public DateTime? ActualArrival { get; set; }

    [Column("status")]
    public string Status { get; set; } = "PENDING"; // PENDING, DONE, COMEBACK, FAILED

    [Column("proof_of_delivery_url")]
    public string? ProofOfDeliveryUrl { get; set; }

    [Column("customer_feedback")]
    public string? CustomerFeedback { get; set; }

    [Column("failure_reason")]
    public string? FailureReason { get; set; }
}
