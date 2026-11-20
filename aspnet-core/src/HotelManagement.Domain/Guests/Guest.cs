using System;
using HotelManagement.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Guests;

public class Guest : FullAuditedAggregateRoot<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public IdDocumentType IdDocumentType { get; set; }
    public string? IdDocumentNumber { get; set; }
    public string? Nationality { get; set; }

    protected Guest()
    {
    }

    public Guest(
        Guid id,
        string firstName,
        string lastName,
        string? email = null,
        string? phone = null,
        IdDocumentType idDocumentType = IdDocumentType.Passport,
        string? idDocumentNumber = null,
        string? nationality = null)
        : base(id)
    {
        FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), maxLength: 64);
        LastName = Check.NotNullOrWhiteSpace(lastName, nameof(lastName), maxLength: 64);
        Email = email;
        Phone = phone;
        IdDocumentType = idDocumentType;
        IdDocumentNumber = idDocumentNumber;
        Nationality = nationality;
    }
}
