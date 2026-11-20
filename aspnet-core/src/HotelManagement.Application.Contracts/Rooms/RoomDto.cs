using System;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Rooms;

public class RoomDto : FullAuditedEntityDto<Guid>
{
    public Guid RoomTypeId { get; set; }
    public string? RoomTypeName { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public RoomStatus Status { get; set; }
}
