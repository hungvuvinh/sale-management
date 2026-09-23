using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaleManagement.Api.Models;
using SaleManagement.Api.Services;

namespace SaleManagement.Api.Controllers;

/// <summary>
/// Delivery Controller — Quản lý Phí vận chuyển, Đặt lịch giao hàng & Delivery Calendar
/// </summary>
[ApiController]
[Route("api/delivery")]
public class DeliveryController : ControllerBase
{
    private readonly DeliveryService _deliveryService;

    public DeliveryController(DeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpGet("service-rates")]
    [AllowAnonymous]
    public async Task<IActionResult> GetServiceRates()
    {
        var rates = await _deliveryService.GetActiveServiceRatesAsync();
        return Ok(new { success = true, data = rates });
    }

    [HttpPut("service-rates/{id:long}")]
    [Authorize]
    public async Task<IActionResult> UpdateServiceRate(long id, [FromBody] UpdateDeliveryServiceRateDto dto)
    {
        var ok = await _deliveryService.UpdateServiceRateAsync(id, dto);
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Delivery service rate with ID {id} not found." });
        }
        return Ok(new { success = true, message = "Delivery service rate updated successfully." });
    }

    [HttpPost("calculate-fee")]
    [AllowAnonymous]
    public async Task<IActionResult> CalculateShippingFee([FromBody] CalculateShippingFeeDto dto)
    {
        var result = await _deliveryService.CalculateShippingFeeAsync(dto);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("bookings")]
    [Authorize]
    public async Task<IActionResult> CreateBooking([FromBody] CreateDeliveryBookingDto dto)
    {
        var booking = await _deliveryService.CreateBookingAsync(dto);
        return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, new { success = true, data = booking });
    }

    [HttpGet("bookings/{id:long}")]
    [Authorize]
    public async Task<IActionResult> GetBookingById(long id)
    {
        var booking = await _deliveryService.GetBookingByIdAsync(id);
        if (booking == null)
        {
            return NotFound(new { success = false, message = $"Delivery booking with ID {id} not found." });
        }
        return Ok(new { success = true, data = booking });
    }

    [HttpGet("calendar")]
    [Authorize]
    public async Task<IActionResult> GetDeliveryCalendar(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] long? storeId)
    {
        var start = startDate ?? DateTime.UtcNow.Date;
        var end = endDate ?? start.AddDays(7);

        var calendar = await _deliveryService.GetDeliveryCalendarAsync(start, end, storeId);
        return Ok(new { success = true, data = calendar });
    }

    [HttpPut("bookings/{id:long}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateBookingStatus(long id, [FromBody] UpdateBookingStatusDto dto)
    {
        var ok = await _deliveryService.UpdateBookingStatusAsync(id, dto.Status);
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Delivery booking with ID {id} not found." });
        }
        return Ok(new { success = true, message = $"Booking status updated to {dto.Status}." });
    }
}

public record UpdateBookingStatusDto(string Status);
