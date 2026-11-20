using System;
using System.ComponentModel.DataAnnotations;
using HotelManagement.Enums;

namespace HotelManagement.Rooms;

public class CreateRoomTaskDto
{
    [Required]
    public Guid RoomId { get; set; }

    public Guid? AssignedUserId { get; set; }

    public RoomTaskType TaskType { get; set; }

    public DateTime ScheduledDate { get; set; }
}
