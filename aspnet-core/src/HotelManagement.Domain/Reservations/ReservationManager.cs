using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagement.Enums;
using HotelManagement.Guests;
using HotelManagement.Rooms;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace HotelManagement.Reservations;

public class ReservationManager : DomainService
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IRepository<ReservationRoom, Guid> _reservationRoomRepository;
    private readonly IRepository<Room, Guid> _roomRepository;
    private readonly IRepository<RoomType, Guid> _roomTypeRepository;
    private readonly IRepository<Guest, Guid> _guestRepository;

    public ReservationManager(
        IRepository<Reservation, Guid> reservationRepository,
        IRepository<ReservationRoom, Guid> reservationRoomRepository,
        IRepository<Room, Guid> roomRepository,
        IRepository<RoomType, Guid> roomTypeRepository,
        IRepository<Guest, Guid> guestRepository)
    {
        _reservationRepository = reservationRepository;
        _reservationRoomRepository = reservationRoomRepository;
        _roomRepository = roomRepository;
        _roomTypeRepository = roomTypeRepository;
        _guestRepository = guestRepository;
    }

    /// <summary>
    /// Règle 1 : Anti-Overbooking dynamique
    /// Vérifie qu'aucune réservation active (Pending, Confirmed, CheckedIn) sur la chambre ne chevauche les dates demandées.
    /// Condition de chevauchement : existing.CheckInDate < requested.CheckOutDate && existing.CheckOutDate > requested.CheckInDate
    /// </summary>
    public async Task<bool> CheckRoomAvailabilityAsync(
        Guid roomId,
        DateTime checkIn,
        DateTime checkOut,
        Guid? excludeReservationId = null)
    {
        if (checkIn >= checkOut)
        {
            throw new BusinessException(
                HotelManagementDomainErrorCodes.InvalidDateRange,
                "Check-out date must be strictly after check-in date."
            );
        }

        var activeStatuses = new[]
        {
            ReservationStatus.Pending,
            ReservationStatus.Confirmed,
            ReservationStatus.CheckedIn
        };

        var reservationQuery = await _reservationRepository.GetQueryableAsync();
        var reservationRoomQuery = await _reservationRoomRepository.GetQueryableAsync();

        var query = from rr in reservationRoomQuery
                    join r in reservationQuery on rr.ReservationId equals r.Id
                    where rr.RoomId == roomId
                          && activeStatuses.Contains(r.Status)
                          && (excludeReservationId == null || r.Id != excludeReservationId.Value)
                          && r.CheckInDate < checkOut
                          && r.CheckOutDate > checkIn
                    select rr;

        var isOverlapping = await AsyncExecuter.AnyAsync(query);

        return !isOverlapping;
    }

    /// <summary>
    /// Création d'une réservation avec validation anti-overbooking, figeage du prix de chaque chambre et calcul de la durée.
    /// </summary>
    public async Task<Reservation> CreateReservationAsync(
        Guid guestId,
        DateTime checkIn,
        DateTime checkOut,
        List<Guid> roomIds,
        int adultsCount,
        int childrenCount = 0,
        string? notes = null)
    {
        if (checkIn >= checkOut)
        {
            throw new BusinessException(
                HotelManagementDomainErrorCodes.InvalidDateRange,
                "Check-out date must be strictly after check-in date."
            );
        }

        if (roomIds == null || !roomIds.Any())
        {
            throw new BusinessException(
                "HotelManagement:NoRoomsSelected",
                "At least one room must be selected to create a reservation."
            );
        }

        // Vérification de l'existence du client
        var guestExists = await _guestRepository.AnyAsync(g => g.Id == guestId);
        if (!guestExists)
        {
            throw new BusinessException(
                HotelManagementDomainErrorCodes.GuestNotFound,
                $"Guest with ID '{guestId}' was not found."
            );
        }

        int nightsCount = Math.Max(1, (int)(checkOut.Date - checkIn.Date).TotalDays);

        // Validation de la disponibilité de chaque chambre
        var roomRates = new Dictionary<Guid, decimal>();
        foreach (var roomId in roomIds.Distinct())
        {
            var isAvailable = await CheckRoomAvailabilityAsync(roomId, checkIn, checkOut);
            if (!isAvailable)
            {
                var busyRoom = await _roomRepository.FindAsync(roomId);
                string roomLabel = busyRoom != null ? busyRoom.RoomNumber : roomId.ToString();
                throw new BusinessException(
                    HotelManagementDomainErrorCodes.RoomNotAvailable,
                    $"Room '{roomLabel}' is not available for the requested period ({checkIn:yyyy-MM-dd} to {checkOut:yyyy-MM-dd})."
                );
            }

            var room = await _roomRepository.GetAsync(roomId);
            var roomType = await _roomTypeRepository.GetAsync(room.RoomTypeId);
            roomRates[roomId] = roomType.BaseNightlyRate;
        }

        // Génération d'un code de réservation unique
        string reservationCode = $"RES-{Clock.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var reservation = new Reservation(
            GuidGenerator.Create(),
            reservationCode,
            guestId,
            checkIn,
            checkOut,
            adultsCount,
            childrenCount,
            ReservationStatus.Pending,
            notes
        );

        foreach (var (roomId, nightlyRate) in roomRates)
        {
            var reservationRoom = new ReservationRoom(
                GuidGenerator.Create(),
                reservation.Id,
                roomId,
                nightlyRate,
                nightsCount
            );
            reservation.Rooms.Add(reservationRoom);
        }

        return await _reservationRepository.InsertAsync(reservation, autoSave: true);
    }
}
