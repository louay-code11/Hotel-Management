using System;
using HotelManagement.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Services;

public class Service : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public ServiceCategory Category { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsActive { get; set; }

    protected Service()
    {
    }

    public Service(Guid id, string name, ServiceCategory category, decimal unitPrice, decimal taxRate, bool isActive = true)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: 128);
        Category = category;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        IsActive = isActive;
    }
}
