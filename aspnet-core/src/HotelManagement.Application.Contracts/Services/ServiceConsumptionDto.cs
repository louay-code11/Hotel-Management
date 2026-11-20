using System;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Services;

public class ServiceConsumptionDto : FullAuditedEntityDto<Guid>
{
    public Guid StayId { get; set; }
    public Guid ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int Quantity { get; set; }
    public DateTime ConsumedAt { get; set; }
    public decimal BilledUnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TotalAmount => Math.Round(BilledUnitPrice * Quantity * (1 + (TaxRate / 100m)), 2);
}
