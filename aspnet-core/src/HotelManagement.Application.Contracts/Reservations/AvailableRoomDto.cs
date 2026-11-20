using System;
using HotelManagement.Rooms;

namespace HotelManagement.Reservations;

public class AvailableRoomDto : RoomDto
{
    public decimal NightlyRate { get; set; }
    public int MaxCapacity { get; set; }
}
