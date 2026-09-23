using Microsoft.EntityFrameworkCore;
using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<PostcodeEntity> Postcodes => Set<PostcodeEntity>();
    public DbSet<AddressEntity> Addresses => Set<AddressEntity>();
    public DbSet<StoreEntity> Stores => Set<StoreEntity>();
    public DbSet<WarehouseEntity> Warehouses => Set<WarehouseEntity>();
    public DbSet<PostcodeStoreEntity> PostcodeStores => Set<PostcodeStoreEntity>();
    public DbSet<StoreShippingRateEntity> StoreShippingRates => Set<StoreShippingRateEntity>();

    public DbSet<ProductCategoryEntity> ProductCategories => Set<ProductCategoryEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<ProductVariantEntity> ProductVariants => Set<ProductVariantEntity>();
    public DbSet<FifoLotStagePriceEntity> FifoLotStagePrices => Set<FifoLotStagePriceEntity>();
    public DbSet<StoreVariantPriceEntity> StoreVariantPrices => Set<StoreVariantPriceEntity>();
    public DbSet<PricingStageConfigEntity> PricingStageConfigs => Set<PricingStageConfigEntity>();
    public DbSet<StagePriceAuditLogEntity> StagePriceAuditLogs => Set<StagePriceAuditLogEntity>();

    public DbSet<SupplierEntity> Suppliers => Set<SupplierEntity>();
    public DbSet<StockOrderEntity> StockOrders => Set<StockOrderEntity>();
    public DbSet<StockOrderItemEntity> StockOrderItems => Set<StockOrderItemEntity>();
    public DbSet<FifoLotEntity> FifoLots => Set<FifoLotEntity>();

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();

    // Day 3 — Customers, Orders & Inventory
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
    public DbSet<OrderItemFifoAllocationEntity> OrderItemFifoAllocations => Set<OrderItemFifoAllocationEntity>();
    public DbSet<OrderPaymentEntity> OrderPayments => Set<OrderPaymentEntity>();
    public DbSet<OrderAuditLogEntity> OrderAuditLogs => Set<OrderAuditLogEntity>();

    public DbSet<WarehouseInventoryEntity> WarehouseInventory => Set<WarehouseInventoryEntity>();
    public DbSet<InventoryTransactionEntity> InventoryTransactions => Set<InventoryTransactionEntity>();
    public DbSet<InventoryTransferEntity> InventoryTransfers => Set<InventoryTransferEntity>();
    public DbSet<InventoryTransferItemEntity> InventoryTransferItems => Set<InventoryTransferItemEntity>();

    // Day 4 — Delivery System
    public DbSet<DeliveryServiceRateEntity> DeliveryServiceRates => Set<DeliveryServiceRateEntity>();
    public DbSet<DeliveryBookingServiceEntity> DeliveryBookingServices => Set<DeliveryBookingServiceEntity>();
    public DbSet<CarrierEntity> Carriers => Set<CarrierEntity>();
    public DbSet<DriverEntity> Drivers => Set<DriverEntity>();
    public DbSet<DeliveryBookingEntity> DeliveryBookings => Set<DeliveryBookingEntity>();
    public DbSet<DeliveryRouteEntity> DeliveryRoutes => Set<DeliveryRouteEntity>();
    public DbSet<DeliveryStopEntity> DeliveryStops => Set<DeliveryStopEntity>();

    // Day 5 — Marketing, Commissions & Combos
    public DbSet<PromotionEntity> Promotions => Set<PromotionEntity>();
    public DbSet<PromotionProductEntity> PromotionProducts => Set<PromotionProductEntity>();
    public DbSet<AdSpendLogEntity> AdSpendLogs => Set<AdSpendLogEntity>();
    public DbSet<SalesShiftEntity> SalesShifts => Set<SalesShiftEntity>();
    public DbSet<SalesCommissionEntity> SalesCommissions => Set<SalesCommissionEntity>();
    public DbSet<ProductComboEntity> ProductCombos => Set<ProductComboEntity>();
    public DbSet<ComboItemEntity> ComboItems => Set<ComboItemEntity>();

    public DbSet<ChatSessionEntity> ChatSessions => Set<ChatSessionEntity>();
    public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PostcodeStoreEntity>()
            .HasKey(ps => new { ps.PostcodeId, ps.StoreId });

        modelBuilder.Entity<PromotionProductEntity>()
            .HasKey(pp => new { pp.PromotionId, pp.ProductId });

        modelBuilder.Entity<FifoLotStagePriceEntity>()
            .HasIndex(p => new { p.FifoLotId, p.Stage })
            .IsUnique();

        modelBuilder.Entity<StoreVariantPriceEntity>()
            .HasIndex(p => new { p.StoreId, p.VariantId })
            .IsUnique();

        // warehouse_inventory has composite PK (warehouse_id, variant_id)
        modelBuilder.Entity<WarehouseInventoryEntity>()
            .HasKey(wi => new { wi.WarehouseId, wi.VariantId });

        // available_quantity is a GENERATED ALWAYS AS column in Postgres — mark as computed
        modelBuilder.Entity<WarehouseInventoryEntity>()
            .Property(wi => wi.AvailableQuantity)
            .ValueGeneratedOnAddOrUpdate();
    }
}
