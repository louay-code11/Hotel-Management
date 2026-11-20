using System;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Rooms;

public class GetRoomListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? RoomTypeId { get; set; }
    public RoomStatus? Status { get; set; }
    public int? Floor { get; set; }
}
