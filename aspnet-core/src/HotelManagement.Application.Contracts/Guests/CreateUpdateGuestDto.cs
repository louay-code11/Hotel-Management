using System.ComponentModel.DataAnnotations;
using HotelManagement.Enums;

namespace HotelManagement.Guests;

public class CreateUpdateGuestDto
{
    [Required]
    [StringLength(64)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(128)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(32)]
    public string? Phone { get; set; }

    public IdDocumentType IdDocumentType { get; set; } = IdDocumentType.Passport;

    [StringLength(64)]
    public string? IdDocumentNumber { get; set; }

    [StringLength(64)]
    public string? Nationality { get; set; }
}
