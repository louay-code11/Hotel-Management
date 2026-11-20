using System;
using System.Collections.Generic;
using HotelManagement.Enums;
using HotelManagement.Stays;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Invoices;

public class Invoice : FullAuditedAggregateRoot<Guid>
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid StayId { get; set; }
    public DateTime IssuedAt { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; }

    public virtual Stay? Stay { get; set; }
    public virtual ICollection<Payment> Payments { get; protected set; } = new List<Payment>();

    protected Invoice()
    {
    }

    public Invoice(
        Guid id,
        string invoiceNumber,
        Guid stayId,
        DateTime issuedAt,
        decimal subtotalAmount,
        decimal taxAmount,
        decimal totalAmount,
        InvoiceStatus status = InvoiceStatus.Draft)
        : base(id)
    {
        InvoiceNumber = Check.NotNullOrWhiteSpace(invoiceNumber, nameof(invoiceNumber), maxLength: 64);
        StayId = stayId;
        IssuedAt = issuedAt;
        SubtotalAmount = subtotalAmount;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
        Status = status;
    }
}
