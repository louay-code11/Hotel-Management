using System;
using HotelManagement.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Rooms;

public class Room : FullAuditedAggregateRoot<Guid>
{
    public Guid RoomTypeId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public RoomStatus Status { get; set; }

    public virtual RoomType? RoomType { get; set; }

    protected Room()
    {
    }

    public Room(Guid id, Guid roomTypeId, string roomNumber, int floor, RoomStatus status = RoomStatus.Available)
        : base(id)
    {
        RoomTypeId = roomTypeId;
        RoomNumber = Check.NotNullOrWhiteSpace(roomNumber, nameof(roomNumber), maxLength: 32);
        Floor = floor;
        Status = status;
    }
}
