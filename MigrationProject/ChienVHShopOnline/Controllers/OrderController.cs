using ChienVHShopOnline.DTOs;
using ChienVHShopOnline.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChienVHShopOnline.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderReadDto>>> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderReadDto>> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportPdf()
    {
        var pdfBytes = await _orderService.ExportOrderListingPdfAsync();
        if (pdfBytes == null)
            return StatusCode(501, "PDF export not implemented in .NET 9");

        return File(pdfBytes, "application/pdf", $"OrderListing_{DateTime.Now:yyyyMMddHHmmss}.pdf");
    }
}
