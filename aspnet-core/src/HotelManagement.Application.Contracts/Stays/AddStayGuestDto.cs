using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Stays;

public class AddStayGuestDto
{
    [Required]
    public Guid GuestId { get; set; }

    public bool IsPrimaryGuest { get; set; } = false;
}
