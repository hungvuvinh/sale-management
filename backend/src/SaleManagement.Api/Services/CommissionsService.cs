using SaleManagement.Api.Data;
using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace SaleManagement.Api.Services;

/// <summary>
/// Commission Engine — computes Seller Commission with Australian Superannuation 9.5%.
/// Algorithm:
///   1. Sum all COMPLETED order totals by salesperson within the period.
///   2. Sum hours worked via sales_shifts.
///   3. Compute sales_per_hour = total_sales / total_hours.
///   4. Target Sales = kpi_threshold_aud * total_hours.
///   5. Excess Sales = max(0, total_sales - target_sales).
///   6. Gross Commission = excess_sales * commission_rate_pct / 100.
///   7. Superannuation = gross_commission * 9.5 / 100.
///   8. Net Commission = gross_commission - superannuation.
/// </summary>
public class CommissionsService
{
    private readonly AppDbContext _db;

    public CommissionsService(AppDbContext db)
    {
        _db = db;
    }

    // ─────────────────────────────────────────────
    //  SHIFTS
    // ─────────────────────────────────────────────

    public async Task<IEnumerable<SalesShiftDto>> GetShiftsAsync(long? userId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _db.SalesShifts
            .Include(s => s.User)
            .Include(s => s.Store)
            .AsQueryable();

        if (userId.HasValue) query = query.Where(s => s.UserId == userId.Value);
        if (fromDate.HasValue) query = query.Where(s => s.WorkDate >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(s => s.WorkDate <= toDate.Value.Date);

        var shifts = await query.OrderByDescending(s => s.WorkDate).ToListAsync();

        return shifts.Select(s => new SalesShiftDto(
            Id: s.Id,
            UserId: s.UserId,
            UserName: s.User != null ? $"{s.User.FirstName} {s.User.LastName}" : string.Empty,
            StoreId: s.StoreId,
            StoreName: s.Store?.Name ?? string.Empty,
            WorkDate: s.WorkDate,
            HoursWorked: s.HoursWorked,
            CreatedAt: s.CreatedAt
        ));
    }

    public async Task<SalesShiftDto> LogShiftAsync(CreateSalesShiftDto dto)
    {
        var entity = new SalesShiftEntity
        {
            UserId = dto.UserId,
            StoreId = dto.StoreId,
            WorkDate = dto.WorkDate.Date,
            HoursWorked = dto.HoursWorked
        };

        _db.SalesShifts.Add(entity);
        await _db.SaveChangesAsync();

        // Reload with navigation
        await _db.Entry(entity).Reference(s => s.User).LoadAsync();
        await _db.Entry(entity).Reference(s => s.Store).LoadAsync();

        return new SalesShiftDto(
            Id: entity.Id,
            UserId: entity.UserId,
            UserName: entity.User != null ? $"{entity.User.FirstName} {entity.User.LastName}" : string.Empty,
            StoreId: entity.StoreId,
            StoreName: entity.Store?.Name ?? string.Empty,
            WorkDate: entity.WorkDate,
            HoursWorked: entity.HoursWorked,
            CreatedAt: entity.CreatedAt
        );
    }

    // ─────────────────────────────────────────────
    //  COMMISSION CALCULATION ENGINE
    // ─────────────────────────────────────────────

    /// <summary>
    /// Calculate and persist commission record for a specific seller within a period.
    /// </summary>
    public async Task<SalesCommissionDto> CalculateCommissionAsync(CalculateCommissionDto dto)
    {
        var periodStart = dto.PeriodStart.Date;
        var periodEnd = dto.PeriodEnd.Date.AddDays(1).AddTicks(-1); // inclusive end-of-day

        // 1. Sum all COMPLETED order totals by salesperson in period
        decimal totalSales = await _db.Orders
            .Where(o => o.SalespersonUserId == dto.UserId
                     && o.Status == "COMPLETED"
                     && o.CreatedAt >= periodStart
                     && o.CreatedAt <= periodEnd)
            .SumAsync(o => (decimal?)o.TotalAud) ?? 0m;

        // 2. Sum hours worked in period
        decimal totalHours = await _db.SalesShifts
            .Where(s => s.UserId == dto.UserId
                     && s.WorkDate >= periodStart
                     && s.WorkDate <= periodEnd)
            .SumAsync(s => (decimal?)s.HoursWorked) ?? 0m;

        // 3. Compute derived values
        decimal salesPerHour = totalHours > 0 ? Math.Round(totalSales / totalHours, 2) : 0m;
        decimal targetSales = Math.Round(dto.KpiThresholdAud * totalHours, 2);
        decimal excessSales = Math.Max(0, totalSales - targetSales);
        decimal grossCommission = Math.Round(excessSales * dto.CommissionRatePct / 100m, 2);
        decimal superannuationPct = dto.SuperannuationPct ?? 9.50m; // AU default 9.5%
        decimal superannuationAud = Math.Round(grossCommission * superannuationPct / 100m, 2);
        decimal netCommission = Math.Round(grossCommission - superannuationAud, 2);

        var entity = new SalesCommissionEntity
        {
            UserId = dto.UserId,
            PeriodStart = periodStart,
            PeriodEnd = dto.PeriodEnd.Date,
            TotalSalesAud = totalSales,
            TotalHoursWorked = totalHours,
            SalesPerHourAud = salesPerHour,
            KpiThresholdAud = dto.KpiThresholdAud,
            TargetSalesAud = targetSales,
            ExcessSalesAud = excessSales,
            CommissionRatePct = dto.CommissionRatePct,
            GrossCommissionAud = grossCommission,
            SuperannuationPct = superannuationPct,
            SuperannuationAud = superannuationAud,
            NetCommissionAud = netCommission,
            Status = "DRAFT"
        };

        _db.SalesCommissions.Add(entity);
        await _db.SaveChangesAsync();

        return MapCommission(entity);
    }

    public async Task<IEnumerable<SalesCommissionDto>> GetCommissionsAsync(long? userId, string? status)
    {
        var query = _db.SalesCommissions
            .Include(c => c.User)
            .AsQueryable();

        if (userId.HasValue) query = query.Where(c => c.UserId == userId.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(c => c.Status == status);

        var commissions = await query.OrderByDescending(c => c.PeriodStart).ToListAsync();
        return commissions.Select(MapCommission);
    }

    public async Task<SalesCommissionDto?> GetCommissionByIdAsync(long id)
    {
        var c = await _db.SalesCommissions
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
        return c != null ? MapCommission(c) : null;
    }

    /// <summary>
    /// Finalize a DRAFT commission record (Admin approval → FINALIZED).
    /// </summary>
    public async Task<bool> FinalizeCommissionAsync(long id)
    {
        var commission = await _db.SalesCommissions.FindAsync(id);
        if (commission == null || commission.Status != "DRAFT") return false;

        commission.Status = "FINALIZED";
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Mark commission as PAID.
    /// </summary>
    public async Task<bool> MarkCommissionPaidAsync(long id)
    {
        var commission = await _db.SalesCommissions.FindAsync(id);
        if (commission == null || commission.Status != "FINALIZED") return false;

        commission.Status = "PAID";
        await _db.SaveChangesAsync();
        return true;
    }

    // ─────────────────────────────────────────────
    //  MAPPING HELPERS
    // ─────────────────────────────────────────────

    private static SalesCommissionDto MapCommission(SalesCommissionEntity c) => new(
        Id: c.Id,
        UserId: c.UserId,
        UserName: c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : string.Empty,
        PeriodStart: c.PeriodStart,
        PeriodEnd: c.PeriodEnd,
        TotalSalesAud: c.TotalSalesAud,
        TotalHoursWorked: c.TotalHoursWorked,
        SalesPerHourAud: c.SalesPerHourAud,
        KpiThresholdAud: c.KpiThresholdAud,
        TargetSalesAud: c.TargetSalesAud,
        ExcessSalesAud: c.ExcessSalesAud,
        CommissionRatePct: c.CommissionRatePct,
        GrossCommissionAud: c.GrossCommissionAud,
        SuperannuationPct: c.SuperannuationPct,
        SuperannuationAud: c.SuperannuationAud,
        NetCommissionAud: c.NetCommissionAud,
        Status: c.Status,
        CreatedAt: c.CreatedAt
    );
}
