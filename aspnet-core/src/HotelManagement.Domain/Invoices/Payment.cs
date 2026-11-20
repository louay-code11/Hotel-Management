using System;
using HotelManagement.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Invoices;

public class Payment : FullAuditedAggregateRoot<Guid>
{
    public Guid InvoiceId { get; set; }
    public DateTime PaidAt { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionRef { get; set; }

    public virtual Invoice? Invoice { get; set; }

    protected Payment()
    {
    }

    public Payment(
        Guid id,
        Guid invoiceId,
        DateTime paidAt,
        decimal amount,
        PaymentMethod paymentMethod,
        string? transactionRef = null)
        : base(id)
    {
        InvoiceId = invoiceId;
        PaidAt = paidAt;
        Amount = amount;
        PaymentMethod = paymentMethod;
        TransactionRef = transactionRef;
    }
}
