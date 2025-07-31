using ChienVHShopOnline.DTOs;
using ChienVHShopOnline.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChienVHShopOnline.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ModelController : ControllerBase
{
    private readonly IModelService _service;

    public ModelController(IModelService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModelDto>>> Get()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ModelDto>> Get(int id)
    {
        var model = await _service.GetByIdAsync(id);
        if (model == null) return NotFound();
        return Ok(model);
    }

    [HttpPost]
    public async Task<ActionResult<ModelDto>> Post(ModelCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.ModelId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, ModelUpdateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
