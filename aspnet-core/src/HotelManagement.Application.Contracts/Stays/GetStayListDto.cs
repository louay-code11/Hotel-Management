using System;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Stays;

public class GetStayListDto : PagedAndSortedResultRequestDto
{
    public StayStatus? Status { get; set; }
    public Guid? RoomId { get; set; }
}
