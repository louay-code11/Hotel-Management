using System;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Stays;

public class StayGuestDto : EntityDto<Guid>
{
    public Guid StayId { get; set; }
    public Guid GuestId { get; set; }
    public string? GuestName { get; set; }
    public bool IsPrimaryGuest { get; set; }
}
