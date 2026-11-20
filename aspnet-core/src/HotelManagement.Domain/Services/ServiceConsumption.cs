using System;
using HotelManagement.Stays;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Services;

public class ServiceConsumption : FullAuditedAggregateRoot<Guid>
{
    public Guid StayId { get; set; }
    public Guid ServiceId { get; set; }
    public int Quantity { get; set; }
    public DateTime ConsumedAt { get; set; }
    public decimal BilledUnitPrice { get; set; }
    public decimal TaxRate { get; set; }

    public virtual Stay? Stay { get; set; }
    public virtual Service? Service { get; set; }

    protected ServiceConsumption()
    {
    }

    public ServiceConsumption(
        Guid id,
        Guid stayId,
        Guid serviceId,
        int quantity,
        DateTime consumedAt,
        decimal billedUnitPrice,
        decimal taxRate)
        : base(id)
    {
        StayId = stayId;
        ServiceId = serviceId;
        Quantity = quantity;
        ConsumedAt = consumedAt;
        BilledUnitPrice = billedUnitPrice;
        TaxRate = taxRate;
    }
}
