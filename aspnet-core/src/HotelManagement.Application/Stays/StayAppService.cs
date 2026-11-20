using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagement.Guests;
using HotelManagement.Permissions;
using HotelManagement.Rooms;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace HotelManagement.Stays;

[Authorize(HotelManagementPermissions.Stays.Default)]
public class StayAppService : ApplicationService, IStayAppService
{
    private readonly IRepository<Stay, Guid> _stayRepository;
    private readonly IRepository<StayGuest, Guid> _stayGuestRepository;
    private readonly IRepository<Room, Guid> _roomRepository;
    private readonly IRepository<Guest, Guid> _guestRepository;
    private readonly IRepository<Service, Guid> _serviceRepository;
    private readonly IRepository<ServiceConsumption, Guid> _consumptionRepository;
    private readonly StayManager _stayManager;

    public StayAppService(
        IRepository<Stay, Guid> stayRepository,
        IRepository<StayGuest, Guid> stayGuestRepository,
        IRepository<Room, Guid> roomRepository,
        IRepository<Guest, Guid> guestRepository,
        IRepository<Service, Guid> serviceRepository,
        IRepository<ServiceConsumption, Guid> consumptionRepository,
        StayManager stayManager)
    {
        _stayRepository = stayRepository;
        _stayGuestRepository = stayGuestRepository;
        _roomRepository = roomRepository;
        _guestRepository = guestRepository;
        _serviceRepository = serviceRepository;
        _consumptionRepository = consumptionRepository;
        _stayManager = stayManager;
    }

    public async Task<StayDto> GetAsync(Guid id)
    {
        var stay = await _stayRepository.GetAsync(id);
        var dto = ObjectMapper.Map<Stay, StayDto>(stay);

        var room = await _roomRepository.FindAsync(stay.RoomId);
        dto.RoomNumber = room?.RoomNumber;

        var stayGuests = await _stayGuestRepository.GetListAsync(sg => sg.StayId == id);
        dto.Guests = ObjectMapper.Map<List<StayGuest>, List<StayGuestDto>>(stayGuests);

        var guestIds = stayGuests.Select(sg => sg.GuestId).Distinct().ToList();
        var guests = await _guestRepository.GetListAsync(g => guestIds.Contains(g.Id));
        var guestDict = guests.ToDictionary(g => g.Id, g => $"{g.FirstName} {g.LastName}".Trim());

        foreach (var guestDto in dto.Guests)
        {
            if (guestDict.TryGetValue(guestDto.GuestId, out var name))
            {
                guestDto.GuestName = name;
            }
        }

        return dto;
    }

    public async Task<PagedResultDto<StayDto>> GetListAsync(GetStayListDto input)
    {
        var query = await _stayRepository.GetQueryableAsync();

        if (input.Status.HasValue)
        {
            query = query.Where(s => s.Status == input.Status.Value);
        }

        if (input.RoomId.HasValue)
        {
            query = query.Where(s => s.RoomId == input.RoomId.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderByDescending(s => s.ActualCheckIn)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var stays = await AsyncExecuter.ToListAsync(query);
        var dtos = ObjectMapper.Map<List<Stay>, List<StayDto>>(stays);

        var roomIds = stays.Select(s => s.RoomId).Distinct().ToList();
        var rooms = await _roomRepository.GetListAsync(r => roomIds.Contains(r.Id));
        var roomDict = rooms.ToDictionary(r => r.Id, r => r.RoomNumber);

        foreach (var dto in dtos)
        {
            if (roomDict.TryGetValue(dto.RoomId, out var number))
            {
                dto.RoomNumber = number;
            }
        }

        return new PagedResultDto<StayDto>(totalCount, dtos);
    }

    [Authorize(HotelManagementPermissions.Stays.CheckOut)]
    public async Task<StayDto> CheckOutAsync(Guid id)
    {
        var stay = await _stayManager.CheckOutAsync(id);
        return await GetAsync(stay.Id);
    }

    [Authorize(HotelManagementPermissions.Stays.AddConsumption)]
    public async Task<ServiceConsumptionDto> AddConsumptionAsync(Guid id, AddConsumptionDto input)
    {
        var stay = await _stayRepository.GetAsync(id);
        if (stay.Status != Enums.StayStatus.Active)
        {
            throw new BusinessException(
                HotelManagementDomainErrorCodes.StayNotActive,
                "Cannot add service consumption to an inactive stay."
            );
        }

        var service = await _serviceRepository.GetAsync(input.ServiceId);
        if (!service.IsActive)
        {
            throw new BusinessException(
                "HotelManagement:ServiceInactive",
                $"Service '{service.Name}' is not currently active."
            );
        }

        var consumption = new ServiceConsumption(
            GuidGenerator.Create(),
            stay.Id,
            service.Id,
            input.Quantity,
            Clock.Now,
            service.UnitPrice,
            service.TaxRate
        );

        await _consumptionRepository.InsertAsync(consumption, autoSave: true);

        var dto = ObjectMapper.Map<ServiceConsumption, ServiceConsumptionDto>(consumption);
        dto.ServiceName = service.Name;
        return dto;
    }

    [Authorize(HotelManagementPermissions.Stays.Edit)]
    public async Task<StayGuestDto> AddGuestAsync(Guid id, AddStayGuestDto input)
    {
        var stay = await _stayRepository.GetAsync(id);
        if (stay.Status != Enums.StayStatus.Active)
        {
            throw new BusinessException(
                HotelManagementDomainErrorCodes.StayNotActive,
                "Cannot add guest to an inactive stay."
            );
        }

        var guest = await _guestRepository.GetAsync(input.GuestId);

        var stayGuest = new StayGuest(
            GuidGenerator.Create(),
            stay.Id,
            guest.Id,
            input.IsPrimaryGuest
        );

        await _stayGuestRepository.InsertAsync(stayGuest, autoSave: true);

        var dto = ObjectMapper.Map<StayGuest, StayGuestDto>(stayGuest);
        dto.GuestName = $"{guest.FirstName} {guest.LastName}".Trim();
        return dto;
    }
}
