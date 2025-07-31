using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChienVHShopOnline.Interfaces;
using ChienVHShopOnline.DTOs;

namespace ChienVHShopOnline.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ColorsController : ControllerBase
{
    private readonly IColorService _colorService;

    public ColorsController(IColorService colorService)
    {
        _colorService = colorService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ColorReadDto>>> GetAll()
    {
        var result = await _colorService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ColorReadDto>> Get(int id)
    {
        var color = await _colorService.GetByIdAsync(id);
        if (color == null) return NotFound();
        return Ok(color);
    }

    [HttpPost]
    public async Task<ActionResult<ColorReadDto>> Create(ColorCreateDto dto)
    {
        var created = await _colorService.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.ColorId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ColorUpdateDto dto)
    {
        var success = await _colorService.UpdateAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _colorService.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
