using System;
using System.Linq;
using System.Threading.Tasks;
using HotelManagement.Enums;
using HotelManagement.Reservations;
using HotelManagement.Rooms;
using HotelManagement.Services;
using HotelManagement.Stays;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace HotelManagement.Invoices;

public class InvoiceManager : DomainService
{
    private readonly IRepository<Invoice, Guid> _invoiceRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<Stay, Guid> _stayRepository;
    private readonly IRepository<ReservationRoom, Guid> _reservationRoomRepository;
    private readonly IRepository<ServiceConsumption, Guid> _consumptionRepository;
    private readonly IRepository<Room, Guid> _roomRepository;
    private readonly IRepository<RoomType, Guid> _roomTypeRepository;

    public InvoiceManager(
        IRepository<Invoice, Guid> invoiceRepository,
        IRepository<Payment, Guid> paymentRepository,
        IRepository<Stay, Guid> stayRepository,
        IRepository<ReservationRoom, Guid> reservationRoomRepository,
        IRepository<ServiceConsumption, Guid> consumptionRepository,
        IRepository<Room, Guid> roomRepository,
        IRepository<RoomType, Guid> roomTypeRepository)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _stayRepository = stayRepository;
        _reservationRoomRepository = reservationRoomRepository;
        _consumptionRepository = consumptionRepository;
        _roomRepository = roomRepository;
        _roomTypeRepository = roomTypeRepository;
    }

    /// <summary>
    /// Règle 3 : Immutabilité comptable (Invoice)
    /// Interdit toute modification des montants dès que Invoice.Status est Issued ou Paid.
    /// Seule l'annulation (Void) est autorisée.
    /// </summary>
    public void EnsureInvoiceIsEditable(Invoice invoice)
    {
        if (invoice.Status == InvoiceStatus.Issued ||
            invoice.Status == InvoiceStatus.Paid ||
            invoice.Status == InvoiceStatus.Void)
        {
            throw new BusinessException(
                HotelManagementDomainErrorCodes.InvoiceImmutable,
                $"Invoice '{invoice.InvoiceNumber}' has status '{invoice.Status}' and amounts cannot be modified. Only Draft invoices are editable."
            );
        }
    }

    /// <summary>
    /// Génère une facture pour un séjour à partir des nuitées réservées et des consommations de services.
    /// </summary>
    public async Task<Invoice> GenerateForStayAsync(Guid stayId, decimal roomTaxRate = 10.0m)
    {
        var stay = await _stayRepository.GetAsync(stayId);

        decimal roomSubtotal = 0m;

        if (stay.ReservationId.HasValue)
        {
            var resRooms = await _reservationRoomRepository.GetListAsync(
                rr => rr.ReservationId == stay.ReservationId.Value && rr.RoomId == stay.RoomId
            );

            var resRoom = resRooms.FirstOrDefault();
            if (resRoom != null)
            {
                roomSubtotal = resRoom.AgreedNightlyRate * resRoom.NightsCount;
            }
        }

        // Si walk-in (pas de réservation liée) ou tarif non trouvé, calculer selon RoomType
        if (roomSubtotal == 0m)
        {
            var room = await _roomRepository.GetAsync(stay.RoomId);
            var roomType = await _roomTypeRepository.GetAsync(room.RoomTypeId);
            var end = stay.ActualCheckOut ?? Clock.Now;
            int nights = Math.Max(1, (int)Math.Ceiling((end - stay.ActualCheckIn).TotalDays));
            roomSubtotal = roomType.BaseNightlyRate * nights;
        }

        decimal roomTax = Math.Round(roomSubtotal * (roomTaxRate / 100m), 2);

        // Calcul des consommations de services
        var consumptions = await _consumptionRepository.GetListAsync(sc => sc.StayId == stayId);
        decimal servicesSubtotal = 0m;
        decimal servicesTax = 0m;

        foreach (var item in consumptions)
        {
            decimal itemSubtotal = Math.Round(item.BilledUnitPrice * item.Quantity, 2);
            decimal itemTax = Math.Round(itemSubtotal * (item.TaxRate / 100m), 2);
            servicesSubtotal += itemSubtotal;
            servicesTax += itemTax;
        }

        decimal subtotalAmount = Math.Round(roomSubtotal + servicesSubtotal, 2);
        decimal taxAmount = Math.Round(roomTax + servicesTax, 2);
        decimal totalAmount = subtotalAmount + taxAmount;

        string invoiceNumber = $"INV-{Clock.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var invoice = new Invoice(
            GuidGenerator.Create(),
            invoiceNumber,
            stayId,
            Clock.Now,
            subtotalAmount,
            taxAmount,
            totalAmount,
            InvoiceStatus.Draft
        );

        return await _invoiceRepository.InsertAsync(invoice, autoSave: true);
    }

    /// <summary>
    /// Règle 4 : Calcul automatique des paiements
    /// Crée le paiement, vérifie la somme des paiements, et passe automatiquement
    /// la facture au statut Paid quand le TotalAmount est atteint.
    /// </summary>
    public async Task<Payment> RecordPaymentAsync(
        Guid invoiceId,
        decimal amount,
        PaymentMethod paymentMethod,
        string? transactionRef = null)
    {
        if (amount <= 0)
        {
            throw new BusinessException(
                "HotelManagement:InvalidPaymentAmount",
                "Payment amount must be greater than zero."
            );
        }

        var invoice = await _invoiceRepository.GetAsync(invoiceId);

        if (invoice.Status == InvoiceStatus.Void)
        {
            throw new BusinessException(
                HotelManagementDomainErrorCodes.InvoiceVoid,
                $"Cannot record payment on voided invoice '{invoice.InvoiceNumber}'."
            );
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new BusinessException(
                "HotelManagement:InvoiceAlreadyPaid",
                $"Invoice '{invoice.InvoiceNumber}' is already fully paid."
            );
        }

        var payment = new Payment(
            GuidGenerator.Create(),
            invoiceId,
            Clock.Now,
            amount,
            paymentMethod,
            transactionRef
        );
        await _paymentRepository.InsertAsync(payment, autoSave: true);

        // Recalcul du total payé
        var allPayments = await _paymentRepository.GetListAsync(p => p.InvoiceId == invoiceId);
        decimal totalPaid = allPayments.Sum(p => p.Amount);

        if (totalPaid >= invoice.TotalAmount)
        {
            invoice.Status = InvoiceStatus.Paid;
            await _invoiceRepository.UpdateAsync(invoice, autoSave: true);
        }
        else if (invoice.Status == InvoiceStatus.Draft)
        {
            invoice.Status = InvoiceStatus.Issued;
            await _invoiceRepository.UpdateAsync(invoice, autoSave: true);
        }

        return payment;
    }

    /// <summary>
    /// Annule une facture (seule opération autorisée sur une facture émise non payée).
    /// </summary>
    public async Task<Invoice> VoidInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _invoiceRepository.GetAsync(invoiceId);

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new BusinessException(
                "HotelManagement:CannotVoidPaidInvoice",
                $"Invoice '{invoice.InvoiceNumber}' is already paid and cannot be voided. A refund must be issued."
            );
        }

        invoice.Status = InvoiceStatus.Void;
        return await _invoiceRepository.UpdateAsync(invoice, autoSave: true);
    }
}
