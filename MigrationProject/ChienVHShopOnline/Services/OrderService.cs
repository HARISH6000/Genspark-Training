using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChienVHShopOnline.Data;
using ChienVHShopOnline.DTOs;
using ChienVHShopOnline.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ChienVHShopOnline.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public OrderService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderReadDto>> GetAllAsync()
    {
        return await _context.Orders
            .OrderByDescending(o => o.OrderID)
            .ProjectTo<OrderReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<OrderReadDto?> GetByIdAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        return order == null ? null : _mapper.Map<OrderReadDto>(order);
    }

    public async Task<byte[]?> ExportOrderListingPdfAsync()
    {
        var orders = await GetAllAsync();

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text("Order Listing Report")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content().Table(table =>
                {
                    // Define columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(60);  // Order ID
                        columns.RelativeColumn();    // User ID
                        columns.RelativeColumn();    // Order Date
                        columns.RelativeColumn();    // Total
                        columns.RelativeColumn();    // Status
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().Text("ID").Bold();
                        header.Cell().Text("User ID").Bold();
                        header.Cell().Text("Order Date").Bold();
                        header.Cell().Text("Total").Bold();
                        header.Cell().Text("Status").Bold();
                    });

                    // Data rows
                    foreach (var order in orders)
                    {
                        table.Cell().Text(order.OrderID.ToString());
                        table.Cell().Text(order.CustomerName ?? "-");
                        table.Cell().Text(order.OrderDate?.ToString("yyyy-MM-dd") ?? "-");
                        table.Cell().Text(order.TotalAmount?.ToString("C") ?? "₹0.00");
                        table.Cell().Text(order.Status);
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    });
            });
        }).GeneratePdf();

        return pdfBytes;
    }

    private string GetStatusText(int? status)
    {
        return status switch
        {
            1 => "Pending",
            2 => "Shipped",
            3 => "Delivered",
            4 => "Cancelled",
            _ => "Unknown"
        };
    }
}
