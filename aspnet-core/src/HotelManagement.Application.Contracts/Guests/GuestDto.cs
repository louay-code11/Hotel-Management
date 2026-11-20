using System;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Guests;

public class GuestDto : FullAuditedEntityDto<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public IdDocumentType IdDocumentType { get; set; }
    public string? IdDocumentNumber { get; set; }
    public string? Nationality { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}
