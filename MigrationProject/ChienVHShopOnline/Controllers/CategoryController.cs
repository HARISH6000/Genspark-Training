using AutoMapper;
using ChienVHShopOnline.DTOs;
using ChienVHShopOnline.Models;
using ChienVHShopOnline.Services;
using ChienVHShopOnline.Interfaces;
using ChienVHShopOnline.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChienVHShopOnline.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;
    private readonly IMapper _mapper;

    public CategoryController(ICategoryService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    // GET: api/Category
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetAll()
    {
        var categories = await _service.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<CategoryReadDto>>(categories));
    }

    // GET: api/Category/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryReadDto>> GetById(int id)
    {
        var category = await _service.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        return Ok(_mapper.Map<CategoryReadDto>(category));
    }

    // POST: api/Category
    [HttpPost]
    public async Task<ActionResult<CategoryReadDto>> Create(CategoryCreateDto dto)
    {
        var category = _mapper.Map<Category>(dto);
        await _service.CreateAsync(category);

        var result = _mapper.Map<CategoryReadDto>(category);
        return CreatedAtAction(nameof(GetById), new { id = result.CategoryId }, result);
    }

    // PUT: api/Category/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryUpdateDto dto)
    {
        if (id != dto.CategoryId)
            return BadRequest("ID mismatch");

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        _mapper.Map(dto, existing);
        await _service.UpdateAsync(existing);

        return Ok(_mapper.Map<CategoryReadDto>(existing));
    }

    // DELETE: api/Category/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await _service.DeleteAsync(existing.CategoryId);
        return Ok();
    }
}
