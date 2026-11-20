using System;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Services;

public class ServiceDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public ServiceCategory Category { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsActive { get; set; }
}
