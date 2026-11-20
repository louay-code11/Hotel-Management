using System;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Rooms;

public class RoomTypeDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public decimal BaseNightlyRate { get; set; }
    public string? Description { get; set; }
}
