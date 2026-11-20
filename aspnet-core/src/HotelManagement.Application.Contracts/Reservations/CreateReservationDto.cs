using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Reservations;

public class CreateReservationDto
{
    [Required]
    public Guid GuestId { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    public List<Guid> RoomIds { get; set; } = new();

    [Range(1, 20)]
    public int AdultsCount { get; set; } = 1;

    [Range(0, 10)]
    public int ChildrenCount { get; set; } = 0;

    [StringLength(2048)]
    public string? Notes { get; set; }
}
