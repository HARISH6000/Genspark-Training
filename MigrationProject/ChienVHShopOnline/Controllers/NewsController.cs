// Controllers/NewsController.cs
using ChienVHShopOnline.DTOs;
using ChienVHShopOnline.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using OfficeOpenXml;


namespace ChienVHShopOnline.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var newsList = await _newsService.GetAllAsync();
        return Ok(newsList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var news = await _newsService.GetByIdAsync(id);
        return news == null ? NotFound() : Ok(news);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<NewsReadDto>>> GetPagedNews([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 2)
    {
        var result = await _newsService.GetPagedNewsAsync(pageNumber, pageSize);
        return Ok(result);
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NewsCreateDto dto)
    {
        var created = await _newsService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.NewsId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] NewsUpdateDto dto)
    {
        if (id != dto.NewsId) return BadRequest("ID mismatch");

        var success = await _newsService.UpdateAsync(dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _newsService.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportToCsv()
    {
        var newsList = await _newsService.GetAllAsync();

        var sb = new StringBuilder();
        sb.AppendLine("NewsId,UserId,Title,ShortDescription,Image,Content,CreatedDate,Status");

        foreach (var news in newsList)
        {
            sb.AppendLine(
                $"{news.NewsId}," +
                $"{news.UserId}," +
                $"\"{news.Title.Replace("\"", "\"\"")}\"," +
                $"\"{news.ShortDescription.Replace("\"", "\"\"")}\"," +
                $"\"{news.Image?.Replace("\"", "\"\"")}\"," +
                $"\"{news.Content.Replace("\"", "\"\"")}\"," +
                $"{news.CreatedDate:yyyy-MM-dd HH:mm:ss}," +
                $"{news.Status}"
            );
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "news-export.csv");
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportToExcel()
    {
        var newsList = await _newsService.GetAllAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("News");

        // Header
        worksheet.Cells[1, 1].Value = "NewsId";
        worksheet.Cells[1, 2].Value = "UserId";
        worksheet.Cells[1, 3].Value = "Title";
        worksheet.Cells[1, 4].Value = "ShortDescription";
        worksheet.Cells[1, 5].Value = "Image";
        worksheet.Cells[1, 6].Value = "Content";
        worksheet.Cells[1, 7].Value = "CreatedDate";
        worksheet.Cells[1, 8].Value = "Status";

        // Data
        int row = 2;
        foreach (var news in newsList)
        {
            worksheet.Cells[row, 1].Value = news.NewsId;
            worksheet.Cells[row, 2].Value = news.UserId;
            worksheet.Cells[row, 3].Value = news.Title;
            worksheet.Cells[row, 4].Value = news.ShortDescription;
            worksheet.Cells[row, 5].Value = news.Image;
            worksheet.Cells[row, 6].Value = news.Content;
            worksheet.Cells[row, 7].Value = news.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            worksheet.Cells[row, 8].Value = news.Status;
            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        var excelBytes = package.GetAsByteArray();
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "news-export.xlsx");
    }
}
