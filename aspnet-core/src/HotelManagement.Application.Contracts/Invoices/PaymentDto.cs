using System;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Invoices;

public class PaymentDto : FullAuditedEntityDto<Guid>
{
    public Guid InvoiceId { get; set; }
    public DateTime PaidAt { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionRef { get; set; }
}
