using System;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Invoices;

public class GetInvoiceListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public InvoiceStatus? Status { get; set; }
    public Guid? StayId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
