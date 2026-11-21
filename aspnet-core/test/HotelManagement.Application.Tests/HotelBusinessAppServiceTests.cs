using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagement.Enums;
using HotelManagement.Guests;
using HotelManagement.Invoices;
using HotelManagement.Reservations;
using HotelManagement.Rooms;
using HotelManagement.Stays;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace HotelManagement.Applications;

public abstract class HotelBusinessAppServiceTests<TStartupModule> : HotelManagementApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected readonly IReservationAppService _reservationAppService;
    protected readonly IStayAppService _stayAppService;
    protected readonly IInvoiceAppService _invoiceAppService;
    protected readonly IRoomAppService _roomAppService;
    protected readonly IRoomTaskAppService _roomTaskAppService;
    protected readonly IGuestAppService _guestAppService;
    protected readonly InvoiceManager _invoiceManager;

    protected HotelBusinessAppServiceTests()
    {
        _reservationAppService = GetRequiredService<IReservationAppService>();
        _stayAppService = GetRequiredService<IStayAppService>();
        _invoiceAppService = GetRequiredService<IInvoiceAppService>();
        _roomAppService = GetRequiredService<IRoomAppService>();
        _roomTaskAppService = GetRequiredService<IRoomTaskAppService>();
        _guestAppService = GetRequiredService<IGuestAppService>();
        _invoiceManager = GetRequiredService<InvoiceManager>();
    }

    /// <summary>
    /// Règle 1 : Anti-Overbooking dynamique
    /// 1. Créer une réservation confirmée du 10 au 15 décembre sur une chambre.
    /// 2. Vérifier qu'une 2e réservation du 12 au 18 décembre sur la même chambre lève une BusinessException (RoomNotAvailable).
    /// 3. Vérifier qu'une réservation du 15 au 18 décembre (débutant le jour même du check-out) est bien acceptée.
    /// </summary>
    [Fact]
    public async Task AntiOverbooking_Should_Prevent_Overlapping_Reservations_And_Allow_Adjacent()
    {
        // Arrange
        var guest = await _guestAppService.CreateAsync(new CreateUpdateGuestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Phone = "+33612345678"
        });

        var roomsResult = await _roomAppService.GetListAsync(new GetRoomListDto());
        var room = roomsResult.Items.First(r => r.RoomNumber == "101");

        var checkIn1 = new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc);
        var checkOut1 = new DateTime(2026, 12, 15, 0, 0, 0, DateTimeKind.Utc);

        // 1. Première réservation du 10 au 15 confirmée
        var reservation1 = await _reservationAppService.CreateAsync(new CreateReservationDto
        {
            GuestId = guest.Id,
            CheckInDate = checkIn1,
            CheckOutDate = checkOut1,
            RoomIds = new List<Guid> { room.Id },
            AdultsCount = 1
        });

        var confirmedRes1 = await _reservationAppService.ConfirmAsync(reservation1.Id);
        confirmedRes1.Status.ShouldBe(ReservationStatus.Confirmed);

        // 2. Deuxième réservation chevauchante du 12 au 18 sur la même chambre -> BusinessException
        var checkIn2 = new DateTime(2026, 12, 12, 0, 0, 0, DateTimeKind.Utc);
        var checkOut2 = new DateTime(2026, 12, 18, 0, 0, 0, DateTimeKind.Utc);

        var ex = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _reservationAppService.CreateAsync(new CreateReservationDto
            {
                GuestId = guest.Id,
                CheckInDate = checkIn2,
                CheckOutDate = checkOut2,
                RoomIds = new List<Guid> { room.Id },
                AdultsCount = 1
            });
        });

        ex.Code.ShouldBe(HotelManagementDomainErrorCodes.RoomNotAvailable);

        // 3. Troisième réservation adjacente du 15 au 18 (même jour que le check-out de la première) -> Succès
        var checkIn3 = new DateTime(2026, 12, 15, 0, 0, 0, DateTimeKind.Utc);
        var checkOut3 = new DateTime(2026, 12, 18, 0, 0, 0, DateTimeKind.Utc);

        var reservation3 = await _reservationAppService.CreateAsync(new CreateReservationDto
        {
            GuestId = guest.Id,
            CheckInDate = checkIn3,
            CheckOutDate = checkOut3,
            RoomIds = new List<Guid> { room.Id },
            AdultsCount = 1
        });

        reservation3.ShouldNotBeNull();
        reservation3.Status.ShouldBe(ReservationStatus.Pending);
        reservation3.Rooms.ShouldContain(r => r.RoomId == room.Id);
    }

    /// <summary>
    /// Règle 2 : Cycle Check-in / Check-out
    /// 1. CheckInAsync crée le Stay actif, associe le client principal et accompagnant dans StayGuest, et passe la chambre à Occupied.
    /// 2. CheckOutAsync passe le Stay à Completed, la chambre à Cleaning, et crée automatiquement une RoomTask de ménage en attente.
    /// </summary>
    [Fact]
    public async Task CheckIn_And_CheckOut_Cycle_Should_Update_Room_Status_And_Create_RoomTask()
    {
        // Arrange
        var primaryGuest = await _guestAppService.CreateAsync(new CreateUpdateGuestDto
        {
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@test.com",
            Phone = "+33612345678"
        });

        var extraGuest = await _guestAppService.CreateAsync(new CreateUpdateGuestDto
        {
            FirstName = "Bob",
            LastName = "Smith",
            Email = "bob@test.com",
            Phone = "+33687654321"
        });

        var roomsResult = await _roomAppService.GetListAsync(new GetRoomListDto());
        var room = roomsResult.Items.First(r => r.RoomNumber == "102");

        var checkIn = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 11, 5, 0, 0, 0, DateTimeKind.Utc);

        var reservation = await _reservationAppService.CreateAsync(new CreateReservationDto
        {
            GuestId = primaryGuest.Id,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            RoomIds = new List<Guid> { room.Id },
            AdultsCount = 2
        });

        await _reservationAppService.ConfirmAsync(reservation.Id);

        // Act 1 : Check-in
        var stays = await _reservationAppService.CheckInAsync(reservation.Id, new CheckInDto
        {
            AdditionalGuestIds = new List<Guid> { extraGuest.Id }
        });

        // Assert 1 : Stay créé, StayGuest associés, et chambre Occupied
        stays.ShouldNotBeNull();
        stays.Count.ShouldBe(1);
        var stayDto = stays.First();
        stayDto.Status.ShouldBe(StayStatus.Active);
        stayDto.RoomId.ShouldBe(room.Id);

        var roomAfterCheckIn = await _roomAppService.GetAsync(room.Id);
        roomAfterCheckIn.Status.ShouldBe(RoomStatus.Occupied);

        var detailedStay = await _stayAppService.GetAsync(stayDto.Id);
        detailedStay.Guests.Count.ShouldBe(2);
        detailedStay.Guests.ShouldContain(g => g.GuestId == primaryGuest.Id && g.IsPrimaryGuest);
        detailedStay.Guests.ShouldContain(g => g.GuestId == extraGuest.Id && !g.IsPrimaryGuest);

        // Act 2 : Check-out
        var checkedOutStay = await _stayAppService.CheckOutAsync(stayDto.Id);

        // Assert 2 : Stay Completed, chambre Cleaning, et RoomTask générée
        checkedOutStay.Status.ShouldBe(StayStatus.Completed);
        checkedOutStay.ActualCheckOut.ShouldNotBeNull();

        var roomAfterCheckOut = await _roomAppService.GetAsync(room.Id);
        roomAfterCheckOut.Status.ShouldBe(RoomStatus.Cleaning);

        var tasksResult = await _roomTaskAppService.GetListAsync(new GetRoomTaskListDto { RoomId = room.Id });
        tasksResult.TotalCount.ShouldBeGreaterThan(0);
        var cleaningTask = tasksResult.Items.FirstOrDefault(t => t.TaskType == RoomTaskType.Cleaning);
        cleaningTask.ShouldNotBeNull();
        cleaningTask.Status.ShouldBe(RoomTaskStatus.Pending);
    }

    /// <summary>
    /// Règles 3 & 4 : Immutabilité comptable et Clôture automatique des paiements
    /// 1. Génération de facture en Draft puis passage à Issued.
    /// 2. Vérifier que la facture Issued est immutable (EnsureInvoiceIsEditable lève une exception).
    /// 3. Enregistrer un paiement partiel -> statut reste Issued.
    /// 4. Enregistrer le solde -> statut passe automatiquement à Paid.
    /// </summary>
    [Fact]
    public async Task Invoice_Immutability_And_Automatic_Payment_Settlement()
    {
        // Arrange
        var guest = await _guestAppService.CreateAsync(new CreateUpdateGuestDto
        {
            FirstName = "Charles",
            LastName = "Dupont",
            Email = "charles.dupont@test.com",
            Phone = "+33611223344"
        });

        var roomsResult = await _roomAppService.GetListAsync(new GetRoomListDto());
        var room = roomsResult.Items.First(r => r.RoomNumber == "103");

        var checkIn = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc); // 3 nuits

        var reservation = await _reservationAppService.CreateAsync(new CreateReservationDto
        {
            GuestId = guest.Id,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            RoomIds = new List<Guid> { room.Id },
            AdultsCount = 1
        });
        await _reservationAppService.ConfirmAsync(reservation.Id);

        var stays = await _reservationAppService.CheckInAsync(reservation.Id, new CheckInDto());
        var stay = stays.First();

        // 1. Générer la facture pour le séjour
        var invoice = await _invoiceAppService.GenerateForStayAsync(stay.Id);
        invoice.ShouldNotBeNull();
        invoice.Status.ShouldBe(InvoiceStatus.Draft);
        invoice.TotalAmount.ShouldBeGreaterThan(0);

        // 2. Émettre la facture (Issued)
        var issuedInvoice = await _invoiceAppService.IssueAsync(invoice.Id);
        issuedInvoice.Status.ShouldBe(InvoiceStatus.Issued);

        // 3. Vérifier l'immutabilité comptable (EnsureInvoiceIsEditable lève une exception sur statut Issued)
        var issuedInvoiceEntity = new Invoice(
            issuedInvoice.Id,
            issuedInvoice.InvoiceNumber,
            issuedInvoice.StayId,
            issuedInvoice.IssuedAt,
            issuedInvoice.SubtotalAmount,
            issuedInvoice.TaxAmount,
            issuedInvoice.TotalAmount,
            InvoiceStatus.Issued
        );
        var immutableEx = Should.Throw<BusinessException>(() =>
        {
            _invoiceManager.EnsureInvoiceIsEditable(issuedInvoiceEntity);
        });
        immutableEx.Code.ShouldBe(HotelManagementDomainErrorCodes.InvoiceImmutable);

        // 4. Paiement partiel -> Statut reste Issued
        decimal halfAmount = Math.Round(invoice.TotalAmount / 2m, 2);
        await _invoiceAppService.RecordPaymentAsync(invoice.Id, new RecordPaymentDto
        {
            Amount = halfAmount,
            PaymentMethod = PaymentMethod.Card,
            TransactionRef = "TXN-PARTIAL-001"
        });

        var invoiceAfterPartial = await _invoiceAppService.GetAsync(invoice.Id);
        invoiceAfterPartial.Status.ShouldBe(InvoiceStatus.Issued);
        invoiceAfterPartial.PaidAmount.ShouldBe(halfAmount);

        // 5. Paiement du solde restant -> Clôture automatique en Paid
        decimal remainingAmount = invoice.TotalAmount - halfAmount;
        await _invoiceAppService.RecordPaymentAsync(invoice.Id, new RecordPaymentDto
        {
            Amount = remainingAmount,
            PaymentMethod = PaymentMethod.Cash,
            TransactionRef = "TXN-FINAL-002"
        });

        var invoiceAfterFullPayment = await _invoiceAppService.GetAsync(invoice.Id);
        invoiceAfterFullPayment.Status.ShouldBe(InvoiceStatus.Paid);
        invoiceAfterFullPayment.PaidAmount.ShouldBe(invoice.TotalAmount);
    }
}
