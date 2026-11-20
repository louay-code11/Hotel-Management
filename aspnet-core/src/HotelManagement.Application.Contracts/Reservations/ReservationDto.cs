using System;
using System.Collections.Generic;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Reservations;

public class ReservationDto : FullAuditedEntityDto<Guid>
{
    public string ReservationCode { get; set; } = string.Empty;
    public Guid GuestId { get; set; }
    public string? GuestName { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int AdultsCount { get; set; }
    public int ChildrenCount { get; set; }
    public ReservationStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<ReservationRoomDto> Rooms { get; set; } = new();
}
