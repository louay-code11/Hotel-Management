using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagement.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace HotelManagement.Rooms;

[Authorize(HotelManagementPermissions.Rooms.Default)]
public class RoomAppService : CrudAppService<
    Room,
    RoomDto,
    Guid,
    GetRoomListDto,
    CreateUpdateRoomDto>,
    IRoomAppService
{
    private readonly IRepository<RoomType, Guid> _roomTypeRepository;

    public RoomAppService(
        IRepository<Room, Guid> repository,
        IRepository<RoomType, Guid> roomTypeRepository)
        : base(repository)
    {
        _roomTypeRepository = roomTypeRepository;
        GetPolicyName = HotelManagementPermissions.Rooms.Default;
        GetListPolicyName = HotelManagementPermissions.Rooms.Default;
        CreatePolicyName = HotelManagementPermissions.Rooms.Create;
        UpdatePolicyName = HotelManagementPermissions.Rooms.Edit;
        DeletePolicyName = HotelManagementPermissions.Rooms.Delete;
    }

    protected override async Task<IQueryable<Room>> CreateFilteredQueryAsync(GetRoomListDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(r => r.RoomNumber.Contains(input.Filter));
        }

        if (input.RoomTypeId.HasValue)
        {
            query = query.Where(r => r.RoomTypeId == input.RoomTypeId.Value);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(r => r.Status == input.Status.Value);
        }

        if (input.Floor.HasValue)
        {
            query = query.Where(r => r.Floor == input.Floor.Value);
        }

        return query;
    }

    protected override async Task<List<RoomDto>> MapToGetListOutputDtosAsync(List<Room> entities)
    {
        var dtos = await base.MapToGetListOutputDtosAsync(entities);

        var roomTypeIds = entities.Select(e => e.RoomTypeId).Distinct().ToList();
        var roomTypes = await _roomTypeRepository.GetListAsync(rt => roomTypeIds.Contains(rt.Id));
        var roomTypeDict = roomTypes.ToDictionary(rt => rt.Id, rt => rt.Name);

        foreach (var dto in dtos)
        {
            if (roomTypeDict.TryGetValue(dto.RoomTypeId, out var typeName))
            {
                dto.RoomTypeName = typeName;
            }
        }

        return dtos;
    }

    protected override async Task<RoomDto> MapToGetOutputDtoAsync(Room entity)
    {
        var dto = await base.MapToGetOutputDtoAsync(entity);
        var roomType = await _roomTypeRepository.FindAsync(entity.RoomTypeId);
        if (roomType != null)
        {
            dto.RoomTypeName = roomType.Name;
        }
        return dto;
    }
}
