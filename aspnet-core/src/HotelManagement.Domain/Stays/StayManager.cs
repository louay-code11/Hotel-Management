using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagement.Enums;
using HotelManagement.Reservations;
using HotelManagement.Rooms;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace HotelManagement.Stays;

public class StayManager : DomainService
{
    private readonly IRepository<Stay, Guid> _stayRepository;
    private readonly IRepository<StayGuest, Guid> _stayGuestRepository;
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IRepository<ReservationRoom, Guid> _reservationRoomRepository;
    private readonly IRepository<Room, Guid> _roomRepository;
    private readonly IRepository<RoomTask, Guid> _roomTaskRepository;

    public StayManager(
        IRepository<Stay, Guid> stayRepository,
        IRepository<StayGuest, Guid> stayGuestRepository,
        IRepository<Reservation, Guid> reservationRepository,
        IRepository<ReservationRoom, Guid> reservationRoomRepository,
        IRepository<Room, Guid> roomRepository,
        IRepository<RoomTask, Guid> roomTaskRepository)
    {
        _stayRepository = stayRepository;
        _stayGuestRepository = stayGuestRepository;
        _reservationRepository = reservationRepository;
        _reservationRoomRepository = reservationRoomRepository;
        _roomRepository = roomRepository;
        _roomTaskRepository = roomTaskRepository;
    }

    /// <summary>
    /// Règle 2 : Conversion Check-in (Reservation -> Stay)
    /// Passe la réservation à CheckedIn, crée un Stay actif pour chaque chambre réservée,
    /// associe le client principal (IsPrimaryGuest = true) et les accompagnants,
    /// et passe le statut de chaque chambre à Occupied.
    /// </summary>
    public async Task<List<Stay>> CheckInFromReservationAsync(
        Guid reservationId,
        List<Guid>? additionalGuestIds = null)
    {
        var reservation = await _reservationRepository.GetAsync(reservationId);

        if (reservation.Status == ReservationStatus.CheckedIn)
        {
            throw new BusinessException(
                HotelManagementDomainErrorCodes.ReservationAlreadyCheckedIn,
                $"Reservation '{reservation.ReservationCode}' is already checked in."
            );
        }

        if (reservation.Status != ReservationStatus.Confirmed)
        {
            throw new BusinessException(
                HotelManagementDomainErrorCodes.ReservationNotConfirmed,
                $"Reservation '{reservation.ReservationCode}' must be in Confirmed status to check in. Current status: {reservation.Status}."
            );
        }

        var reservationRooms = await _reservationRoomRepository.GetListAsync(rr => rr.ReservationId == reservationId);
        if (!reservationRooms.Any())
        {
            throw new BusinessException(
                "HotelManagement:ReservationHasNoRooms",
                $"Reservation '{reservation.ReservationCode}' has no rooms associated."
            );
        }

        var now = Clock.Now;
        reservation.Status = ReservationStatus.CheckedIn;
        await _reservationRepository.UpdateAsync(reservation, autoSave: true);

        var createdStays = new List<Stay>();

        foreach (var rr in reservationRooms)
        {
            // 1. Création du séjour actif
            var stay = new Stay(
                GuidGenerator.Create(),
                reservation.Id,
                rr.RoomId,
                now,
                StayStatus.Active
            );
            await _stayRepository.InsertAsync(stay, autoSave: true);

            // 2. Association du client principal
            var primaryStayGuest = new StayGuest(
                GuidGenerator.Create(),
                stay.Id,
                reservation.GuestId,
                isPrimaryGuest: true
            );
            await _stayGuestRepository.InsertAsync(primaryStayGuest);

            // 3. Association des clients accompagnants
            if (additionalGuestIds != null && additionalGuestIds.Any())
            {
                foreach (var extraGuestId in additionalGuestIds.Distinct())
                {
                    if (extraGuestId != reservation.GuestId)
                    {
                        var extraGuest = new StayGuest(
                            GuidGenerator.Create(),
                            stay.Id,
                            extraGuestId,
                            isPrimaryGuest: false
                        );
                        await _stayGuestRepository.InsertAsync(extraGuest);
                    }
                }
            }

            // 4. Passage de la chambre à Occupied
            var room = await _roomRepository.GetAsync(rr.RoomId);
            room.Status = RoomStatus.Occupied;
            await _roomRepository.UpdateAsync(room, autoSave: true);

            createdStays.Add(stay);
        }

        return createdStays;
    }

    /// <summary>
    /// Check-out du séjour : passe le Stay à Completed, met la chambre à Cleaning
    /// et crée automatiquement une tâche ménage (Cleaning) pour la chambre.
    /// </summary>
    public async Task<Stay> CheckOutAsync(Guid stayId)
    {
        var stay = await _stayRepository.GetAsync(stayId);

        if (stay.Status != StayStatus.Active)
        {
            throw new BusinessException(
                HotelManagementDomainErrorCodes.StayNotActive,
                $"Stay '{stay.Id}' is not active. Current status: {stay.Status}."
            );
        }

        var now = Clock.Now;
        stay.Status = StayStatus.Completed;
        stay.ActualCheckOut = now;
        await _stayRepository.UpdateAsync(stay, autoSave: true);

        // Passage de la chambre à Cleaning
        var room = await _roomRepository.GetAsync(stay.RoomId);
        room.Status = RoomStatus.Cleaning;
        await _roomRepository.UpdateAsync(room, autoSave: true);

        // Création automatique de la tâche ménage
        var cleaningTask = new RoomTask(
            GuidGenerator.Create(),
            room.Id,
            RoomTaskType.Cleaning,
            now,
            assignedUserId: null,
            status: RoomTaskStatus.Pending
        );
        await _roomTaskRepository.InsertAsync(cleaningTask, autoSave: true);

        return stay;
    }
}
