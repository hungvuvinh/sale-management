using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaleManagement.Api.Models;
using SaleManagement.Api.Services;
using System.Security.Claims;

namespace SaleManagement.Api.Controllers;

/// <summary>
/// POS Orders Controller — tạo đơn, tra cứu đơn, cập nhật trạng thái, ghi nhận thanh toán.
/// </summary>
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrdersService _ordersService;

    public OrdersController(OrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    private long? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(claim, out var id) ? id : null;
    }

    // GET /api/orders?storeId=1&status=CONFIRMED&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderListItemDto>>> GetOrders(
        [FromQuery] long? storeId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        var orders = await _ordersService.GetOrdersAsync(storeId, status, page, pageSize);
        return Ok(new { data = orders, page, pageSize });
    }

    // GET /api/orders/{id}
    [HttpGet("{id:long}")]
    public async Task<ActionResult<OrderDto>> GetOrder(long id)
    {
        var order = await _ordersService.GetOrderByIdAsync(id);
        if (order == null) return NotFound(new { message = $"Không tìm thấy đơn hàng ID {id}." });
        return Ok(order);
    }

    // POST /api/orders
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest(new { message = "Đơn hàng phải có ít nhất 1 sản phẩm." });

        try
        {
            var actorId = GetCurrentUserId();
            var created = await _ordersService.CreateOrderAsync(dto.Items.ToArray(), dto, actorId);
            return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PATCH /api/orders/{id}/status
    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            var actorId = GetCurrentUserId();
            var updated = await _ordersService.UpdateOrderStatusAsync(id, dto.Status, actorId);
            if (!updated) return NotFound(new { message = $"Không tìm thấy đơn hàng ID {id}." });
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/orders/{id}/payments
    [HttpPost("{id:long}/payments")]
    public async Task<ActionResult<OrderPaymentDto>> AddPayment(long id, [FromBody] AddPaymentDto dto)
    {
        try
        {
            var actorId = GetCurrentUserId();
            var payment = await _ordersService.AddPaymentAsync(id, dto, actorId);
            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/// <summary>
/// Customers Controller — quản lý hồ sơ khách hàng VIP Loyalty.
/// </summary>
[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly OrdersService _ordersService;

    public CustomersController(OrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    // GET /api/customers?phone=0412&email=...
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers(
        [FromQuery] string? phone,
        [FromQuery] string? email)
    {
        var customers = await _ordersService.GetCustomersAsync(phone, email);
        return Ok(customers);
    }

    // GET /api/customers/{id}
    [HttpGet("{id:long}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(long id)
    {
        var customer = await _ordersService.GetCustomerByIdAsync(id);
        if (customer == null) return NotFound(new { message = $"Không tìm thấy khách hàng ID {id}." });
        return Ok(customer);
    }

    // POST /api/customers
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        try
        {
            var created = await _ordersService.CreateCustomerAsync(dto);
            return CreatedAtAction(nameof(GetCustomer), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
