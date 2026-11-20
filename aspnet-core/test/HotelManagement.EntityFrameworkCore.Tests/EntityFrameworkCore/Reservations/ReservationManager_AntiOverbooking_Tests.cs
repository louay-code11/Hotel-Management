using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Enums;
using HotelManagement.Guests;
using HotelManagement.Reservations;
using HotelManagement.Rooms;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace HotelManagement.EntityFrameworkCore.Reservations;

/// <summary>
/// Tests d'intégration – Règle Anti-Overbooking (Étape 9 / Test 1)
/// Vérifie que ReservationManager.CheckRoomAvailabilityAsync et CreateReservationAsync
/// empêchent correctement le double-booking et acceptent les séjours en limite de dates.
/// </summary>
[Collection(HotelManagementTestConsts.CollectionDefinitionName)]
public class ReservationManager_AntiOverbooking_Tests : HotelManagementEntityFrameworkCoreTestBase
{
    private readonly ReservationManager _reservationManager;
    private readonly IRepository<RoomType, Guid> _roomTypeRepo;
    private readonly IRepository<Room, Guid> _roomRepo;
    private readonly IRepository<Guest, Guid> _guestRepo;
    private readonly IRepository<Reservation, Guid> _reservationRepo;

    public ReservationManager_AntiOverbooking_Tests()
    {
        _reservationManager = GetRequiredService<ReservationManager>();
        _roomTypeRepo       = GetRequiredService<IRepository<RoomType, Guid>>();
        _roomRepo           = GetRequiredService<IRepository<Room, Guid>>();
        _guestRepo          = GetRequiredService<IRepository<Guest, Guid>>();
        _reservationRepo    = GetRequiredService<IRepository<Reservation, Guid>>();
    }

    // ─────────────────────────── helpers ───────────────────────────

    private async Task<(Guid guestId, Guid roomId)> SeedMinimalDataAsync(string suffix = "")
    {
        var roomType = new RoomType(Guid.NewGuid(), $"Standard{suffix}", 2, 100m);
        await _roomTypeRepo.InsertAsync(roomType, autoSave: true);

        var room = new Room(Guid.NewGuid(), roomType.Id, $"OB{suffix}", 1);
        await _roomRepo.InsertAsync(room, autoSave: true);

        var guest = new Guest(Guid.NewGuid(), "Alice", $"Smith{suffix}", $"alice{suffix}@test.com");
        await _guestRepo.InsertAsync(guest, autoSave: true);

        return (guest.Id, room.Id);
    }

    // ─────────────────────────── tests ───────────────────────────

    /// <summary>
    /// Test 1a – Une réservation qui chevauche une réservation confirmée doit lever BusinessException.
    /// Réservation existante : 10→15 décembre.
    /// Tentative             : 12→18 décembre  → overlap  → doit échouer.
    /// </summary>
    [Fact]
    public async Task CreateReservation_ShouldThrow_WhenRoomOverlaps()
    {
        // Arrange
        var (guestId, roomId) = await SeedMinimalDataAsync("A");

        // Première réservation : 10→15 décembre (Pending par défaut)
        // On la confirme manuellement pour simuler l'état réel
        var first = await _reservationManager.CreateReservationAsync(
            guestId,
            new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 15, 0, 0, 0, DateTimeKind.Utc),
            new List<Guid> { roomId },
            adultsCount: 1
        );
        first.Status = ReservationStatus.Confirmed;
        await _reservationRepo.UpdateAsync(first, autoSave: true);

        // Act & Assert – tentative 12→18 sur la même chambre
        var ex = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _reservationManager.CreateReservationAsync(
                guestId,
                new DateTime(2026, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 12, 18, 0, 0, 0, DateTimeKind.Utc),
                new List<Guid> { roomId },
                adultsCount: 1
            );
        });

        ex.Code.ShouldBe(HotelManagementDomainErrorCodes.RoomNotAvailable);
    }

    /// <summary>
    /// Test 1b – Une réservation qui commence exactement au jour du check-out d'une réservation
    /// existante NE DOIT PAS être bloquée (frontière exclusive).
    /// Réservation existante : 10→15 décembre.
    /// Tentative             : 15→18 décembre  → aucun overlap → doit réussir.
    /// </summary>
    [Fact]
    public async Task CreateReservation_ShouldSucceed_WhenStartsOnCheckoutDate()
    {
        // Arrange
        var (guestId, roomId) = await SeedMinimalDataAsync("B");

        var first = await _reservationManager.CreateReservationAsync(
            guestId,
            new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 15, 0, 0, 0, DateTimeKind.Utc),
            new List<Guid> { roomId },
            adultsCount: 1
        );
        first.Status = ReservationStatus.Confirmed;
        await _reservationRepo.UpdateAsync(first, autoSave: true);

        // Act – 15→18 décembre (commence exactement au checkout de la première)
        var second = await _reservationManager.CreateReservationAsync(
            guestId,
            new DateTime(2026, 12, 15, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 18, 0, 0, 0, DateTimeKind.Utc),
            new List<Guid> { roomId },
            adultsCount: 1
        );

        // Assert
        second.ShouldNotBeNull();
        second.ReservationCode.ShouldStartWith("RES-");
    }

    /// <summary>
    /// Test 1c – CheckRoomAvailabilityAsync retourne true quand aucune réservation active n'existe.
    /// </summary>
    [Fact]
    public async Task CheckAvailability_ShouldReturnTrue_WhenNoReservation()
    {
        // Arrange
        var (_, roomId) = await SeedMinimalDataAsync("C");

        // Act
        var available = await _reservationManager.CheckRoomAvailabilityAsync(
            roomId,
            new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 5, 0, 0, 0, DateTimeKind.Utc)
        );

        // Assert
        available.ShouldBeTrue();
    }

    /// <summary>
    /// Test 1d – Dates invalides (checkOut <= checkIn) doivent lever BusinessException:InvalidDateRange.
    /// </summary>
    [Fact]
    public async Task CreateReservation_ShouldThrow_WhenDatesAreInvalid()
    {
        var (guestId, roomId) = await SeedMinimalDataAsync("D");

        var ex = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _reservationManager.CreateReservationAsync(
                guestId,
                new DateTime(2026, 12, 15, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc), // checkOut avant checkIn
                new List<Guid> { roomId },
                adultsCount: 1
            );
        });

        ex.Code.ShouldBe(HotelManagementDomainErrorCodes.InvalidDateRange);
    }
}
