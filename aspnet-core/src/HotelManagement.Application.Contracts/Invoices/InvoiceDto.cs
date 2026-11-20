using System;
using System.Collections.Generic;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Invoices;

public class InvoiceDto : FullAuditedEntityDto<Guid>
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid StayId { get; set; }
    public DateTime IssuedAt { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => Math.Max(0, TotalAmount - PaidAmount);
}
