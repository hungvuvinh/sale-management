using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaleManagement.Api.Models;
using SaleManagement.Api.Services;

namespace SaleManagement.Api.Controllers;

/// <summary>
/// API Hoa hồng Nhân viên Bán hàng (Seller Commission Engine)
/// Endpoints:
///   GET    /api/commissions/shifts          — List work shifts
///   POST   /api/commissions/shifts          — Log a work shift
///   GET    /api/commissions                 — List commission records
///   GET    /api/commissions/{id}            — Get commission by ID
///   POST   /api/commissions/calculate       — Calculate commission for a period
///   PUT    /api/commissions/{id}/finalize   — Admin: Finalize (DRAFT → FINALIZED)
///   PUT    /api/commissions/{id}/pay        — Admin: Mark as PAID
/// </summary>
[ApiController]
[Route("api/commissions")]
[Authorize]
public class CommissionsController : ControllerBase
{
    private readonly CommissionsService _service;

    public CommissionsController(CommissionsService service)
    {
        _service = service;
    }

    // ─────────────────────────────────────────────
    //  SHIFTS
    // ─────────────────────────────────────────────

    /// <summary>GET /api/commissions/shifts — List work shifts with optional filters</summary>
    [HttpGet("shifts")]
    public async Task<IActionResult> GetShifts(
        [FromQuery] long? userId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var shifts = await _service.GetShiftsAsync(userId, fromDate, toDate);
        return Ok(shifts);
    }

    /// <summary>POST /api/commissions/shifts — Log a new work shift</summary>
    [HttpPost("shifts")]
    public async Task<IActionResult> LogShift([FromBody] CreateSalesShiftDto dto)
    {
        if (dto.HoursWorked <= 0)
            return BadRequest(new { error = "Số giờ làm việc phải lớn hơn 0." });

        var shift = await _service.LogShiftAsync(dto);
        return CreatedAtAction(nameof(GetShifts), new { userId = shift.UserId }, shift);
    }

    // ─────────────────────────────────────────────
    //  COMMISSIONS
    // ─────────────────────────────────────────────

    /// <summary>GET /api/commissions — List all commission records</summary>
    [HttpGet]
    public async Task<IActionResult> GetCommissions(
        [FromQuery] long? userId,
        [FromQuery] string? status)
    {
        var commissions = await _service.GetCommissionsAsync(userId, status);
        return Ok(commissions);
    }

    /// <summary>GET /api/commissions/{id} — Get commission by ID</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetCommissionById([FromRoute] long id)
    {
        var commission = await _service.GetCommissionByIdAsync(id);
        if (commission == null) return NotFound(new { error = $"Không tìm thấy hoa hồng ID {id}." });
        return Ok(commission);
    }

    /// <summary>
    /// POST /api/commissions/calculate — Run commission calculation for a seller in a period.
    /// Body: { userId, periodStart, periodEnd, kpiThresholdAud, commissionRatePct, superannuationPct? }
    /// </summary>
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateCommission([FromBody] CalculateCommissionDto dto)
    {
        if (dto.PeriodStart > dto.PeriodEnd)
            return BadRequest(new { error = "Ngày bắt đầu kỳ phải trước ngày kết thúc kỳ." });

        if (dto.KpiThresholdAud < 0 || dto.CommissionRatePct < 0)
            return BadRequest(new { error = "KPI threshold và tỷ lệ hoa hồng không được âm." });

        var result = await _service.CalculateCommissionAsync(dto);
        return CreatedAtAction(nameof(GetCommissionById), new { id = result.Id }, result);
    }

    /// <summary>PUT /api/commissions/{id}/finalize — Admin finalizes a DRAFT commission</summary>
    [HttpPut("{id:long}/finalize")]
    public async Task<IActionResult> FinalizeCommission([FromRoute] long id)
    {
        var ok = await _service.FinalizeCommissionAsync(id);
        if (!ok) return BadRequest(new { error = "Không thể finalize. Kiểm tra lại ID hoặc trạng thái (phải là DRAFT)." });
        return Ok(new { message = $"Commission #{id} đã được FINALIZED." });
    }

    /// <summary>PUT /api/commissions/{id}/pay — Admin marks a FINALIZED commission as PAID</summary>
    [HttpPut("{id:long}/pay")]
    public async Task<IActionResult> MarkPaid([FromRoute] long id)
    {
        var ok = await _service.MarkCommissionPaidAsync(id);
        if (!ok) return BadRequest(new { error = "Không thể đánh dấu PAID. Kiểm tra lại ID hoặc trạng thái (phải là FINALIZED)." });
        return Ok(new { message = $"Commission #{id} đã được đánh dấu PAID." });
    }
}
