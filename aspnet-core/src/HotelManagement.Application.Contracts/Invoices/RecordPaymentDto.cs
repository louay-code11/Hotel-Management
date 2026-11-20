using System.ComponentModel.DataAnnotations;
using HotelManagement.Enums;

namespace HotelManagement.Invoices;

public class RecordPaymentDto
{
    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Card;

    [StringLength(128)]
    public string? TransactionRef { get; set; }
}
