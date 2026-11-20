using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Rooms;

public class RoomType : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public decimal BaseNightlyRate { get; set; }
    public string? Description { get; set; }

    protected RoomType()
    {
    }

    public RoomType(Guid id, string name, int maxCapacity, decimal baseNightlyRate, string? description = null)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: 128);
        MaxCapacity = maxCapacity;
        BaseNightlyRate = baseNightlyRate;
        Description = description;
    }
}
