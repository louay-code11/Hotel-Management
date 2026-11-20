using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Rooms;

public class CreateUpdateRoomTypeDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 20)]
    public int MaxCapacity { get; set; }

    [Range(0, 100000)]
    public decimal BaseNightlyRate { get; set; }

    [StringLength(1024)]
    public string? Description { get; set; }
}
