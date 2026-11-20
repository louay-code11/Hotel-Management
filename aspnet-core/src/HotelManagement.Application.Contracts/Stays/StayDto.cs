using System;
using System.Collections.Generic;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Stays;

public class StayDto : FullAuditedEntityDto<Guid>
{
    public Guid? ReservationId { get; set; }
    public Guid RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public DateTime ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }
    public StayStatus Status { get; set; }
    public List<StayGuestDto> Guests { get; set; } = new();
}
