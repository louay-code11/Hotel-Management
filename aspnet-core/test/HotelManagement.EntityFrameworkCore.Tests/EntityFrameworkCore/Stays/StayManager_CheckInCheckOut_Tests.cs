using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagement.Enums;
using HotelManagement.Guests;
using HotelManagement.Reservations;
using HotelManagement.Rooms;
using HotelManagement.Stays;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace HotelManagement.EntityFrameworkCore.Stays;

/// <summary>
/// Tests d'intégration – Cycle Check-in / Check-out (Étape 9 / Test 2)
/// Vérifie que StayManager crée correctement le Stay, les StayGuests,
/// met la chambre à Occupied au check-in, puis à Cleaning + RoomTask au check-out.
/// </summary>
[Collection(HotelManagementTestConsts.CollectionDefinitionName)]
public class StayManager_CheckInCheckOut_Tests : HotelManagementEntityFrameworkCoreTestBase
{
    private readonly ReservationManager _reservationManager;
    private readonly StayManager _stayManager;
    private readonly IRepository<RoomType, Guid> _roomTypeRepo;
    private readonly IRepository<Room, Guid> _roomRepo;
    private readonly IRepository<Guest, Guid> _guestRepo;
    private readonly IRepository<Reservation, Guid> _reservationRepo;
    private readonly IRepository<Stay, Guid> _stayRepo;
    private readonly IRepository<StayGuest, Guid> _stayGuestRepo;
    private readonly IRepository<RoomTask, Guid> _roomTaskRepo;

    public StayManager_CheckInCheckOut_Tests()
    {
        _reservationManager = GetRequiredService<ReservationManager>();
        _stayManager        = GetRequiredService<StayManager>();
        _roomTypeRepo       = GetRequiredService<IRepository<RoomType, Guid>>();
        _roomRepo           = GetRequiredService<IRepository<Room, Guid>>();
        _guestRepo          = GetRequiredService<IRepository<Guest, Guid>>();
        _reservationRepo    = GetRequiredService<IRepository<Reservation, Guid>>();
        _stayRepo           = GetRequiredService<IRepository<Stay, Guid>>();
        _stayGuestRepo      = GetRequiredService<IRepository<StayGuest, Guid>>();
        _roomTaskRepo       = GetRequiredService<IRepository<RoomTask, Guid>>();
    }

    // ─────────────────────────── helpers ───────────────────────────

    /// <summary>Insère les données de base et renvoie une réservation en statut Confirmed.</summary>
    private async Task<(Reservation reservation, Guid roomId, Guid guestId)> SeedConfirmedReservationAsync()
    {
        var roomType = new RoomType(Guid.NewGuid(), "Deluxe-CI", 2, 150m);
        await _roomTypeRepo.InsertAsync(roomType, autoSave: true);

        var room = new Room(Guid.NewGuid(), roomType.Id, $"CI{Guid.NewGuid().ToString()[..4]}", 2);
        await _roomRepo.InsertAsync(room, autoSave: true);

        var guest = new Guest(Guid.NewGuid(), "Bob", "Martin", "bob@test.com");
        await _guestRepo.InsertAsync(guest, autoSave: true);

        var reservation = await _reservationManager.CreateReservationAsync(
            guest.Id,
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(3),
            new List<Guid> { room.Id },
            adultsCount: 1
        );

        // Passe la réservation à Confirmed (prérequis du check-in)
        reservation.Status = ReservationStatus.Confirmed;
        await _reservationRepo.UpdateAsync(reservation, autoSave: true);

        return (reservation, room.Id, guest.Id);
    }

    // ─────────────────────────── tests ───────────────────────────

    /// <summary>
    /// Test 2a – CheckInFromReservationAsync doit :
    ///   - créer un Stay avec Status = Active
    ///   - créer un StayGuest avec IsPrimaryGuest = true pour le client principal
    ///   - mettre la Room.Status à Occupied
    /// </summary>
    [Fact]
    public async Task CheckIn_ShouldCreateStay_WithPrimaryGuest_And_OccupiedRoom()
    {
        // Arrange
        var (reservation, roomId, guestId) = await SeedConfirmedReservationAsync();

        // Act
        var stays = await _stayManager.CheckInFromReservationAsync(reservation.Id);

        // Assert – au moins un séjour créé
        stays.ShouldNotBeNull();
        stays.ShouldNotBeEmpty();

        var stay = stays.First();

        // Stay status = Active
        stay.Status.ShouldBe(StayStatus.Active);
        stay.ActualCheckIn.ShouldNotBe(default);

        // StayGuest principal existant
        var stayGuests = await _stayGuestRepo.GetListAsync(sg => sg.StayId == stay.Id);
        stayGuests.ShouldNotBeEmpty();

        var primaryGuest = stayGuests.FirstOrDefault(sg => sg.IsPrimaryGuest);
        primaryGuest.ShouldNotBeNull();
        primaryGuest!.GuestId.ShouldBe(guestId);

        // Room.Status = Occupied
        var room = await _roomRepo.GetAsync(roomId);
        room.Status.ShouldBe(RoomStatus.Occupied);
    }

    /// <summary>
    /// Test 2b – CheckInFromReservationAsync doit aussi enregistrer les accompagnants.
    /// </summary>
    [Fact]
    public async Task CheckIn_ShouldRegisterAdditionalGuests()
    {
        // Arrange
        var (reservation, _, _) = await SeedConfirmedReservationAsync();

        var extraGuest = new Guest(Guid.NewGuid(), "Carol", "White", "carol@test.com");
        await _guestRepo.InsertAsync(extraGuest, autoSave: true);

        // Act
        var stays = await _stayManager.CheckInFromReservationAsync(
            reservation.Id,
            additionalGuestIds: new List<Guid> { extraGuest.Id }
        );

        var stayId = stays.First().Id;

        // Assert – 2 StayGuests (principal + accompagnant)
        var stayGuests = await _stayGuestRepo.GetListAsync(sg => sg.StayId == stayId);
        stayGuests.Count.ShouldBe(2);
        stayGuests.ShouldContain(sg => sg.GuestId == extraGuest.Id && !sg.IsPrimaryGuest);
    }

    /// <summary>
    /// Test 2c – CheckInFromReservationAsync doit lever BusinessException si la réservation est Pending.
    /// </summary>
    [Fact]
    public async Task CheckIn_ShouldThrow_WhenReservationNotConfirmed()
    {
        // Arrange – réservation en statut Pending (défaut après création)
        var roomType = new RoomType(Guid.NewGuid(), "Eco-CI", 1, 80m);
        await _roomTypeRepo.InsertAsync(roomType, autoSave: true);

        var room = new Room(Guid.NewGuid(), roomType.Id, $"ECO{Guid.NewGuid().ToString()[..4]}", 1);
        await _roomRepo.InsertAsync(room, autoSave: true);

        var guest = new Guest(Guid.NewGuid(), "Dave", "Green", "dave@test.com");
        await _guestRepo.InsertAsync(guest, autoSave: true);

        var reservation = await _reservationManager.CreateReservationAsync(
            guest.Id,
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(2),
            new List<Guid> { room.Id },
            adultsCount: 1
        );
        // Status reste Pending

        // Act & Assert
        var ex = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _stayManager.CheckInFromReservationAsync(reservation.Id);
        });

        ex.Code.ShouldBe(HotelManagementDomainErrorCodes.ReservationNotConfirmed);
    }

    /// <summary>
    /// Test 2d – CheckOutAsync doit :
    ///   - passer Stay.Status à Completed avec ActualCheckOut renseigné
    ///   - mettre Room.Status à Cleaning
    ///   - créer automatiquement une RoomTask de type Cleaning / Pending
    /// </summary>
    [Fact]
    public async Task CheckOut_ShouldCompleteStay_And_CreateCleaningTask()
    {
        // Arrange – check-in d'abord
        var (reservation, roomId, _) = await SeedConfirmedReservationAsync();
        var stays = await _stayManager.CheckInFromReservationAsync(reservation.Id);
        var stay = stays.First();

        // Act
        await _stayManager.CheckOutAsync(stay.Id);

        // Assert – Stay complété
        var updatedStay = await _stayRepo.GetAsync(stay.Id);
        updatedStay.Status.ShouldBe(StayStatus.Completed);
        updatedStay.ActualCheckOut.ShouldNotBeNull();

        // Room.Status = Cleaning
        var room = await _roomRepo.GetAsync(roomId);
        room.Status.ShouldBe(RoomStatus.Cleaning);

        // RoomTask Cleaning automatique
        var tasks = await _roomTaskRepo.GetListAsync(rt => rt.RoomId == roomId);
        tasks.ShouldNotBeEmpty();

        var cleaningTask = tasks.FirstOrDefault(rt => rt.TaskType == RoomTaskType.Cleaning);
        cleaningTask.ShouldNotBeNull();
        cleaningTask!.Status.ShouldBe(RoomTaskStatus.Pending);
    }

    /// <summary>
    /// Test 2e – CheckOutAsync doit lever BusinessException si le Stay n'est pas Active.
    /// </summary>
    [Fact]
    public async Task CheckOut_ShouldThrow_WhenStayAlreadyCompleted()
    {
        // Arrange
        var (reservation, _, _) = await SeedConfirmedReservationAsync();
        var stays = await _stayManager.CheckInFromReservationAsync(reservation.Id);
        var stay = stays.First();

        await _stayManager.CheckOutAsync(stay.Id); // premier checkout OK

        // Act & Assert – deuxième checkout interdit
        var ex = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _stayManager.CheckOutAsync(stay.Id);
        });

        ex.Code.ShouldBe(HotelManagementDomainErrorCodes.StayNotActive);
    }
}
