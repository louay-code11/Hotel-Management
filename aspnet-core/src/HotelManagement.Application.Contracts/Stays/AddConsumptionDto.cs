using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Stays;

public class AddConsumptionDto
{
    [Required]
    public Guid ServiceId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;
}
