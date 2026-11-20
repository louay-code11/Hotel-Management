using System;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Reservations;

public class ReservationRoomDto : EntityDto<Guid>
{
    public Guid ReservationId { get; set; }
    public Guid RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public decimal AgreedNightlyRate { get; set; }
    public int NightsCount { get; set; }
    public decimal TotalPrice => AgreedNightlyRate * NightsCount;
}
