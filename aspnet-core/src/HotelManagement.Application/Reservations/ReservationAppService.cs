using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagement.Enums;
using HotelManagement.Guests;
using HotelManagement.Permissions;
using HotelManagement.Rooms;
using HotelManagement.Stays;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace HotelManagement.Reservations;

[Authorize(HotelManagementPermissions.Reservations.Default)]
public class ReservationAppService : ApplicationService, IReservationAppService
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IRepository<ReservationRoom, Guid> _reservationRoomRepository;
    private readonly IRepository<Room, Guid> _roomRepository;
    private readonly IRepository<RoomType, Guid> _roomTypeRepository;
    private readonly IRepository<Guest, Guid> _guestRepository;
    private readonly ReservationManager _reservationManager;
    private readonly StayManager _stayManager;

    public ReservationAppService(
        IRepository<Reservation, Guid> reservationRepository,
        IRepository<ReservationRoom, Guid> reservationRoomRepository,
        IRepository<Room, Guid> roomRepository,
        IRepository<RoomType, Guid> roomTypeRepository,
        IRepository<Guest, Guid> guestRepository,
        ReservationManager reservationManager,
        StayManager stayManager)
    {
        _reservationRepository = reservationRepository;
        _reservationRoomRepository = reservationRoomRepository;
        _roomRepository = roomRepository;
        _roomTypeRepository = roomTypeRepository;
        _guestRepository = guestRepository;
        _reservationManager = reservationManager;
        _stayManager = stayManager;
    }

    public async Task<ReservationDto> GetAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id);
        var dto = ObjectMapper.Map<Reservation, ReservationDto>(reservation);

        var rooms = await _reservationRoomRepository.GetListAsync(rr => rr.ReservationId == id);
        dto.Rooms = ObjectMapper.Map<List<ReservationRoom>, List<ReservationRoomDto>>(rooms);

        // Populate room numbers
        var roomIds = rooms.Select(r => r.RoomId).Distinct().ToList();
        var roomList = await _roomRepository.GetListAsync(r => roomIds.Contains(r.Id));
        var roomDict = roomList.ToDictionary(r => r.Id, r => r.RoomNumber);

        foreach (var roomDto in dto.Rooms)
        {
            if (roomDict.TryGetValue(roomDto.RoomId, out var number))
            {
                roomDto.RoomNumber = number;
            }
        }

        return dto;
    }

    public async Task<PagedResultDto<ReservationDto>> GetListAsync(GetReservationListDto input)
    {
        var query = await _reservationRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(r => r.ReservationCode.Contains(input.Filter));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(r => r.Status == input.Status.Value);
        }

        if (input.GuestId.HasValue)
        {
            query = query.Where(r => r.GuestId == input.GuestId.Value);
        }

        if (input.FromDate.HasValue)
        {
            query = query.Where(r => r.CheckInDate >= input.FromDate.Value);
        }

        if (input.ToDate.HasValue)
        {
            query = query.Where(r => r.CheckOutDate <= input.ToDate.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderByDescending(r => r.CheckInDate)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var reservations = await AsyncExecuter.ToListAsync(query);
        var dtos = ObjectMapper.Map<List<Reservation>, List<ReservationDto>>(reservations);

        // Populate Guest names
        var guestIds = reservations.Select(r => r.GuestId).Distinct().ToList();
        var guests = await _guestRepository.GetListAsync(g => guestIds.Contains(g.Id));
        var guestDict = guests.ToDictionary(g => g.Id, g => $"{g.FirstName} {g.LastName}".Trim());

        foreach (var dto in dtos)
        {
            if (guestDict.TryGetValue(dto.GuestId, out var name))
            {
                dto.GuestName = name;
            }
        }

        return new PagedResultDto<ReservationDto>(totalCount, dtos);
    }

    [Authorize(HotelManagementPermissions.Reservations.Create)]
    public async Task<ReservationDto> CreateAsync(CreateReservationDto input)
    {
        var reservation = await _reservationManager.CreateReservationAsync(
            input.GuestId,
            input.CheckInDate,
            input.CheckOutDate,
            input.RoomIds,
            input.AdultsCount,
            input.ChildrenCount,
            input.Notes
        );

        return await GetAsync(reservation.Id);
    }

    [Authorize(HotelManagementPermissions.Reservations.Edit)]
    public async Task<ReservationDto> ConfirmAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id);

        if (reservation.Status != ReservationStatus.Pending)
        {
            throw new BusinessException(
                "HotelManagement:ReservationCannotBeConfirmed",
                $"Only reservations with Pending status can be confirmed. Current status: {reservation.Status}."
            );
        }

        var resRooms = await _reservationRoomRepository.GetListAsync(rr => rr.ReservationId == id);
        foreach (var rr in resRooms)
        {
            var isAvailable = await _reservationManager.CheckRoomAvailabilityAsync(
                rr.RoomId,
                reservation.CheckInDate,
                reservation.CheckOutDate,
                excludeReservationId: reservation.Id
            );

            if (!isAvailable)
            {
                var busyRoom = await _roomRepository.FindAsync(rr.RoomId);
                throw new BusinessException(
                    HotelManagementDomainErrorCodes.RoomNotAvailable,
                    $"Room '{(busyRoom != null ? busyRoom.RoomNumber : rr.RoomId.ToString())}' is no longer available."
                );
            }
        }

        reservation.Status = ReservationStatus.Confirmed;
        await _reservationRepository.UpdateAsync(reservation, autoSave: true);

        return await GetAsync(id);
    }

    [Authorize(HotelManagementPermissions.Reservations.Cancel)]
    public async Task<ReservationDto> CancelAsync(Guid id)
    {
        var reservation = await _reservationRepository.GetAsync(id);

        if (reservation.Status == ReservationStatus.CheckedIn)
        {
            throw new BusinessException(
                "HotelManagement:CannotCancelCheckedInReservation",
                "Cannot cancel a reservation that is already checked in. Check-out must be performed instead."
            );
        }

        reservation.Status = ReservationStatus.Cancelled;
        await _reservationRepository.UpdateAsync(reservation, autoSave: true);

        return await GetAsync(id);
    }

    public async Task<List<AvailableRoomDto>> GetAvailableRoomsAsync(
        DateTime checkIn,
        DateTime checkOut,
        Guid? roomTypeId = null)
    {
        var roomsQuery = await _roomRepository.GetQueryableAsync();

        if (roomTypeId.HasValue)
        {
            roomsQuery = roomsQuery.Where(r => r.RoomTypeId == roomTypeId.Value);
        }

        var allRooms = await AsyncExecuter.ToListAsync(roomsQuery);
        var roomTypes = await _roomTypeRepository.GetListAsync();
        var roomTypeDict = roomTypes.ToDictionary(rt => rt.Id, rt => rt);

        var availableRooms = new List<AvailableRoomDto>();

        foreach (var room in allRooms)
        {
            var isAvailable = await _reservationManager.CheckRoomAvailabilityAsync(room.Id, checkIn, checkOut);
            if (isAvailable && roomTypeDict.TryGetValue(room.RoomTypeId, out var type))
            {
                availableRooms.Add(new AvailableRoomDto
                {
                    Id = room.Id,
                    RoomTypeId = room.RoomTypeId,
                    RoomTypeName = type.Name,
                    RoomNumber = room.RoomNumber,
                    Floor = room.Floor,
                    Status = room.Status,
                    NightlyRate = type.BaseNightlyRate,
                    MaxCapacity = type.MaxCapacity
                });
            }
        }

        return availableRooms;
    }

    [Authorize(HotelManagementPermissions.Reservations.CheckIn)]
    public async Task<List<StayDto>> CheckInAsync(Guid id, CheckInDto input)
    {
        var stays = await _stayManager.CheckInFromReservationAsync(id, input.AdditionalGuestIds);
        return ObjectMapper.Map<List<Stay>, List<StayDto>>(stays);
    }
}
