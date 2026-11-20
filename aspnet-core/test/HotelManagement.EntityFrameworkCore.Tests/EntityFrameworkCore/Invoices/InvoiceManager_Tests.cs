using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Enums;
using HotelManagement.Guests;
using HotelManagement.Invoices;
using HotelManagement.Reservations;
using HotelManagement.Rooms;
using HotelManagement.Stays;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace HotelManagement.EntityFrameworkCore.Invoices;

/// <summary>
/// Tests d'intégration – Immutabilité de facture &amp; Auto-paiement (Étape 9 / Test 3)
/// Vérifie :
///   - EnsureInvoiceIsEditable lève BusinessException sur une facture Issued/Paid/Void
///   - RecordPaymentAsync n'auto-passe pas la facture à Paid avant d'atteindre TotalAmount
///   - RecordPaymentAsync passe automatiquement la facture à Paid quand totalPaid >= TotalAmount
/// </summary>
[Collection(HotelManagementTestConsts.CollectionDefinitionName)]
public class InvoiceManager_Tests : HotelManagementEntityFrameworkCoreTestBase
{
    private readonly InvoiceManager _invoiceManager;
    private readonly StayManager _stayManager;
    private readonly ReservationManager _reservationManager;
    private readonly IRepository<Invoice, Guid> _invoiceRepo;
    private readonly IRepository<RoomType, Guid> _roomTypeRepo;
    private readonly IRepository<Room, Guid> _roomRepo;
    private readonly IRepository<Guest, Guid> _guestRepo;
    private readonly IRepository<Reservation, Guid> _reservationRepo;
    private readonly IRepository<Stay, Guid> _stayRepo;

    public InvoiceManager_Tests()
    {
        _invoiceManager     = GetRequiredService<InvoiceManager>();
        _stayManager        = GetRequiredService<StayManager>();
        _reservationManager = GetRequiredService<ReservationManager>();
        _invoiceRepo        = GetRequiredService<IRepository<Invoice, Guid>>();
        _roomTypeRepo       = GetRequiredService<IRepository<RoomType, Guid>>();
        _roomRepo           = GetRequiredService<IRepository<Room, Guid>>();
        _guestRepo          = GetRequiredService<IRepository<Guest, Guid>>();
        _reservationRepo    = GetRequiredService<IRepository<Reservation, Guid>>();
        _stayRepo           = GetRequiredService<IRepository<Stay, Guid>>();
    }

    // ─────────────────────────── helpers ───────────────────────────

    /// <summary>Crée un Stay actif minimal et renvoie son Id pour la génération de facture.</summary>
    private async Task<Guid> SeedActiveStayAsync(string suffix = "")
    {
        var roomType = new RoomType(Guid.NewGuid(), $"InvType{suffix}", 2, 100m);
        await _roomTypeRepo.InsertAsync(roomType, autoSave: true);

        var room = new Room(Guid.NewGuid(), roomType.Id, $"INV{suffix}", 1);
        await _roomRepo.InsertAsync(room, autoSave: true);

        var guest = new Guest(Guid.NewGuid(), "Eve", $"Brown{suffix}", $"eve{suffix}@test.com");
        await _guestRepo.InsertAsync(guest, autoSave: true);

        var reservation = await _reservationManager.CreateReservationAsync(
            guest.Id,
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(2),
            new List<Guid> { room.Id },
            adultsCount: 1
        );
        reservation.Status = ReservationStatus.Confirmed;
        await _reservationRepo.UpdateAsync(reservation, autoSave: true);

        var stays = await _stayManager.CheckInFromReservationAsync(reservation.Id);
        return stays[0].Id;
    }

    /// <summary>Insère directement une facture avec le statut voulu (sans passer par le générateur).</summary>
    private async Task<Invoice> InsertInvoiceDirectlyAsync(InvoiceStatus status, decimal totalAmount = 100m)
    {
        // On a besoin d'un Stay valide pour respecter la FK
        var stayId = await SeedActiveStayAsync(Guid.NewGuid().ToString()[..4]);

        var invoice = new Invoice(
            Guid.NewGuid(),
            $"INV-TEST-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            stayId,
            DateTime.UtcNow,
            subtotalAmount: Math.Round(totalAmount / 1.1m, 2),
            taxAmount: Math.Round(totalAmount - totalAmount / 1.1m, 2),
            totalAmount: totalAmount,
            status: status
        );

        return await _invoiceRepo.InsertAsync(invoice, autoSave: true);
    }

    // ─────────────────────────── tests immutabilité ───────────────────────────

    /// <summary>
    /// Test 3a – EnsureInvoiceIsEditable doit lever BusinessException:InvoiceImmutable
    /// pour une facture au statut Issued.
    /// </summary>
    [Fact]
    public async Task EnsureEditable_ShouldThrow_WhenInvoiceIsIssued()
    {
        var invoice = await InsertInvoiceDirectlyAsync(InvoiceStatus.Issued);

        var ex = Should.Throw<BusinessException>(() =>
        {
            _invoiceManager.EnsureInvoiceIsEditable(invoice);
        });

        ex.Code.ShouldBe(HotelManagementDomainErrorCodes.InvoiceImmutable);
    }

    /// <summary>
    /// Test 3b – EnsureInvoiceIsEditable doit lever BusinessException:InvoiceImmutable
    /// pour une facture au statut Paid.
    /// </summary>
    [Fact]
    public async Task EnsureEditable_ShouldThrow_WhenInvoiceIsPaid()
    {
        var invoice = await InsertInvoiceDirectlyAsync(InvoiceStatus.Paid);

        var ex = Should.Throw<BusinessException>(() =>
        {
            _invoiceManager.EnsureInvoiceIsEditable(invoice);
        });

        ex.Code.ShouldBe(HotelManagementDomainErrorCodes.InvoiceImmutable);
    }

    /// <summary>
    /// Test 3c – EnsureInvoiceIsEditable doit lever BusinessException:InvoiceImmutable
    /// pour une facture au statut Void.
    /// </summary>
    [Fact]
    public async Task EnsureEditable_ShouldThrow_WhenInvoiceIsVoid()
    {
        var invoice = await InsertInvoiceDirectlyAsync(InvoiceStatus.Void);

        var ex = Should.Throw<BusinessException>(() =>
        {
            _invoiceManager.EnsureInvoiceIsEditable(invoice);
        });

        ex.Code.ShouldBe(HotelManagementDomainErrorCodes.InvoiceImmutable);
    }

    /// <summary>
    /// Test 3d – EnsureInvoiceIsEditable ne doit PAS lever d'exception pour une facture Draft.
    /// </summary>
    [Fact]
    public async Task EnsureEditable_ShouldNotThrow_WhenInvoiceIsDraft()
    {
        var invoice = await InsertInvoiceDirectlyAsync(InvoiceStatus.Draft);

        Should.NotThrow(() => _invoiceManager.EnsureInvoiceIsEditable(invoice));
    }

    // ─────────────────────────── tests auto-paiement ───────────────────────────

    /// <summary>
    /// Test 3e – Premier paiement partiel (60/100) : la facture reste Issued / Draft (non Paid).
    /// Le statut passe de Draft à Issued dès le premier paiement partiel.
    /// </summary>
    [Fact]
    public async Task RecordPayment_ShouldNotMarkAsPaid_WhenPartialPayment()
    {
        // Arrange
        var invoice = await InsertInvoiceDirectlyAsync(InvoiceStatus.Draft, totalAmount: 100m);

        // Act – paiement partiel 60€
        await _invoiceManager.RecordPaymentAsync(invoice.Id, 60m, PaymentMethod.Card);

        // Assert – facture non encore payée
        var updated = await _invoiceRepo.GetAsync(invoice.Id);
        updated.Status.ShouldNotBe(InvoiceStatus.Paid);
        // Après un premier paiement partiel, le statut doit être Issued
        updated.Status.ShouldBe(InvoiceStatus.Issued);
    }

    /// <summary>
    /// Test 3f – Second paiement qui atteint le total (40€ → total = 100€) :
    /// la facture doit automatiquement passer à Paid.
    /// </summary>
    [Fact]
    public async Task RecordPayment_ShouldMarkAsPaid_WhenTotalReached()
    {
        // Arrange
        var invoice = await InsertInvoiceDirectlyAsync(InvoiceStatus.Draft, totalAmount: 100m);

        // Act – 60€ puis 40€
        await _invoiceManager.RecordPaymentAsync(invoice.Id, 60m, PaymentMethod.Card);
        await _invoiceManager.RecordPaymentAsync(invoice.Id, 40m, PaymentMethod.Cash);

        // Assert – facture auto-marquée Paid
        var paid = await _invoiceRepo.GetAsync(invoice.Id);
        paid.Status.ShouldBe(InvoiceStatus.Paid);
    }

    /// <summary>
    /// Test 3g – Un paiement unique qui dépasse le total doit aussi marquer la facture Paid.
    /// </summary>
    [Fact]
    public async Task RecordPayment_ShouldMarkAsPaid_WhenOverpaid()
    {
        // Arrange
        var invoice = await InsertInvoiceDirectlyAsync(InvoiceStatus.Draft, totalAmount: 100m);

        // Act – paiement supérieur au total
        await _invoiceManager.RecordPaymentAsync(invoice.Id, 150m, PaymentMethod.BankTransfer);

        // Assert
        var paid = await _invoiceRepo.GetAsync(invoice.Id);
        paid.Status.ShouldBe(InvoiceStatus.Paid);
    }

    /// <summary>
    /// Test 3h – Tenter d'encaisser un paiement sur une facture Void doit lever BusinessException.
    /// </summary>
    [Fact]
    public async Task RecordPayment_ShouldThrow_WhenInvoiceIsVoid()
    {
        var invoice = await InsertInvoiceDirectlyAsync(InvoiceStatus.Void, totalAmount: 100m);

        var ex = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _invoiceManager.RecordPaymentAsync(invoice.Id, 50m, PaymentMethod.Cash);
        });

        ex.Code.ShouldBe(HotelManagementDomainErrorCodes.InvoiceVoid);
    }

    // ─────────────────────────── test génération facture ───────────────────────────

    /// <summary>
    /// Test 3i – GenerateForStayAsync doit produire une facture Draft avec InvoiceNumber non vide.
    /// </summary>
    [Fact]
    public async Task GenerateForStay_ShouldCreateDraftInvoice()
    {
        // Arrange – Stay actif avec 2 nuits × 100€ = 200€ ht (via réservation)
        var stayId = await SeedActiveStayAsync("GEN");

        // Act
        var invoice = await _invoiceManager.GenerateForStayAsync(stayId);

        // Assert
        invoice.ShouldNotBeNull();
        invoice.InvoiceNumber.ShouldStartWith("INV-");
        invoice.Status.ShouldBe(InvoiceStatus.Draft);
        invoice.TotalAmount.ShouldBeGreaterThan(0m);
    }
}
