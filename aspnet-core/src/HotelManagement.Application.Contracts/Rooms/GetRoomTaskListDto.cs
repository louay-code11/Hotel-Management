using System;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Rooms;

public class GetRoomTaskListDto : PagedAndSortedResultRequestDto
{
    public Guid? RoomId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public RoomTaskType? TaskType { get; set; }
    public RoomTaskStatus? Status { get; set; }
}
