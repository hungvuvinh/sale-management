using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaleManagement.Api.Models;
using SaleManagement.Api.Services;

namespace SaleManagement.Api.Controllers;

/// <summary>
/// API Marketing: Promotions, Product Combos (Cross-sell) &amp; Ad Spend
/// Endpoints:
///   GET    /api/marketing/promotions         — List promotions
///   GET    /api/marketing/promotions/{id}    — Get promotion by ID
///   POST   /api/marketing/promotions         — Create promotion
///   PUT    /api/marketing/promotions/{id}    — Update promotion
///   DELETE /api/marketing/promotions/{id}    — Delete promotion
///
///   GET    /api/marketing/combos             — List product combos
///   GET    /api/marketing/combos/{id}        — Get combo by ID
///   POST   /api/marketing/combos             — Create combo
///   PUT    /api/marketing/combos/{id}        — Update combo
///   DELETE /api/marketing/combos/{id}        — Delete combo
///
///   GET    /api/marketing/ad-spends          — List ad spend logs
///   POST   /api/marketing/ad-spends          — Create ad spend log
/// </summary>
[ApiController]
[Route("api/marketing")]
[Authorize]
public class MarketingController : ControllerBase
{
    private readonly MarketingService _service;

    public MarketingController(MarketingService service)
    {
        _service = service;
    }

    // ─────────────────────────────────────────────
    //  PROMOTIONS
    // ─────────────────────────────────────────────

    /// <summary>GET /api/marketing/promotions — List promotions (optional: activeOnly=true)</summary>
    [HttpGet("promotions")]
    public async Task<IActionResult> GetPromotions([FromQuery] bool? activeOnly)
    {
        var promotions = await _service.GetPromotionsAsync(activeOnly);
        return Ok(promotions);
    }

    /// <summary>GET /api/marketing/promotions/{id} — Get promotion by ID</summary>
    [HttpGet("promotions/{id:long}")]
    public async Task<IActionResult> GetPromotionById([FromRoute] long id)
    {
        var promotion = await _service.GetPromotionByIdAsync(id);
        if (promotion == null) return NotFound(new { error = $"Không tìm thấy promotion ID {id}." });
        return Ok(promotion);
    }

    /// <summary>POST /api/marketing/promotions — Create new promotion</summary>
    [HttpPost("promotions")]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionDto dto)
    {
        if (dto.DiscountValue <= 0)
            return BadRequest(new { error = "Giá trị giảm giá phải lớn hơn 0." });

        if (!new[] { "PERCENTAGE", "FIXED_AMOUNT" }.Contains(dto.DiscountType))
            return BadRequest(new { error = "discountType phải là PERCENTAGE hoặc FIXED_AMOUNT." });

        try
        {
            var promotion = await _service.CreatePromotionAsync(dto);
            return CreatedAtAction(nameof(GetPromotionById), new { id = promotion.Id }, promotion);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>PUT /api/marketing/promotions/{id} — Update promotion</summary>
    [HttpPut("promotions/{id:long}")]
    public async Task<IActionResult> UpdatePromotion([FromRoute] long id, [FromBody] UpdatePromotionDto dto)
    {
        var ok = await _service.UpdatePromotionAsync(id, dto);
        if (!ok) return NotFound(new { error = $"Không tìm thấy promotion ID {id}." });
        return NoContent();
    }

    /// <summary>DELETE /api/marketing/promotions/{id} — Delete promotion</summary>
    [HttpDelete("promotions/{id:long}")]
    public async Task<IActionResult> DeletePromotion([FromRoute] long id)
    {
        var ok = await _service.DeletePromotionAsync(id);
        if (!ok) return NotFound(new { error = $"Không tìm thấy promotion ID {id}." });
        return NoContent();
    }

    // ─────────────────────────────────────────────
    //  PRODUCT COMBOS
    // ─────────────────────────────────────────────

    /// <summary>GET /api/marketing/combos — List product combos</summary>
    [HttpGet("combos")]
    public async Task<IActionResult> GetCombos([FromQuery] bool? activeOnly)
    {
        var combos = await _service.GetCombosAsync(activeOnly);
        return Ok(combos);
    }

    /// <summary>GET /api/marketing/combos/{id} — Get combo by ID</summary>
    [HttpGet("combos/{id:long}")]
    public async Task<IActionResult> GetComboById([FromRoute] long id)
    {
        var combo = await _service.GetComboByIdAsync(id);
        if (combo == null) return NotFound(new { error = $"Không tìm thấy combo ID {id}." });
        return Ok(combo);
    }

    /// <summary>POST /api/marketing/combos — Create new product combo</summary>
    [HttpPost("combos")]
    public async Task<IActionResult> CreateCombo([FromBody] CreateProductComboDto dto)
    {
        if (dto.ComboPriceAud < 0)
            return BadRequest(new { error = "Giá Combo không được âm." });

        try
        {
            var combo = await _service.CreateComboAsync(dto);
            return CreatedAtAction(nameof(GetComboById), new { id = combo.Id }, combo);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>PUT /api/marketing/combos/{id} — Update combo</summary>
    [HttpPut("combos/{id:long}")]
    public async Task<IActionResult> UpdateCombo([FromRoute] long id, [FromBody] UpdateProductComboDto dto)
    {
        var ok = await _service.UpdateComboAsync(id, dto);
        if (!ok) return NotFound(new { error = $"Không tìm thấy combo ID {id}." });
        return NoContent();
    }

    /// <summary>DELETE /api/marketing/combos/{id} — Delete combo</summary>
    [HttpDelete("combos/{id:long}")]
    public async Task<IActionResult> DeleteCombo([FromRoute] long id)
    {
        var ok = await _service.DeleteComboAsync(id);
        if (!ok) return NotFound(new { error = $"Không tìm thấy combo ID {id}." });
        return NoContent();
    }

    // ─────────────────────────────────────────────
    //  AD SPEND LOGS
    // ─────────────────────────────────────────────

    /// <summary>GET /api/marketing/ad-spends — List ad spend logs</summary>
    [HttpGet("ad-spends")]
    public async Task<IActionResult> GetAdSpendLogs(
        [FromQuery] string? platform,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var logs = await _service.GetAdSpendLogsAsync(platform, fromDate, toDate);
        return Ok(logs);
    }

    /// <summary>POST /api/marketing/ad-spends — Create new ad spend log</summary>
    [HttpPost("ad-spends")]
    public async Task<IActionResult> CreateAdSpendLog([FromBody] CreateAdSpendLogDto dto)
    {
        if (dto.SpendAud < 0)
            return BadRequest(new { error = "Chi phí quảng cáo không được âm." });

        var validPlatforms = new[] { "FACEBOOK", "GOOGLE", "TIKTOK", "OTHER" };
        if (!validPlatforms.Contains(dto.Platform.ToUpperInvariant()))
            return BadRequest(new { error = $"Platform phải là một trong: {string.Join(", ", validPlatforms)}." });

        var log = await _service.CreateAdSpendLogAsync(dto);
        return Created($"/api/marketing/ad-spends/{log.Id}", log);
    }
}
