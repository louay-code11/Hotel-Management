using System;
using System.ComponentModel.DataAnnotations;
using HotelManagement.Enums;

namespace HotelManagement.Rooms;

public class CreateUpdateRoomDto
{
    [Required]
    public Guid RoomTypeId { get; set; }

    [Required]
    [StringLength(32)]
    public string RoomNumber { get; set; } = string.Empty;

    public int Floor { get; set; }

    public RoomStatus Status { get; set; } = RoomStatus.Available;
}
