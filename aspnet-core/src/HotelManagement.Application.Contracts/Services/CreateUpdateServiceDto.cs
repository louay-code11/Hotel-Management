using System.ComponentModel.DataAnnotations;
using HotelManagement.Enums;

namespace HotelManagement.Services;

public class CreateUpdateServiceDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    public ServiceCategory Category { get; set; } = ServiceCategory.Other;

    [Range(0, 100000)]
    public decimal UnitPrice { get; set; }

    [Range(0, 100)]
    public decimal TaxRate { get; set; } = 20.0m;

    public bool IsActive { get; set; } = true;
}
