using System;
using HotelManagement.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace HotelManagement.Rooms;

public class RoomTypeAppService : CrudAppService<
    RoomType,
    RoomTypeDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateRoomTypeDto>,
    IRoomTypeAppService
{
    public RoomTypeAppService(IRepository<RoomType, Guid> repository)
        : base(repository)
    {
        GetPolicyName = HotelManagementPermissions.RoomTypes.Default;
        GetListPolicyName = HotelManagementPermissions.RoomTypes.Default;
        CreatePolicyName = HotelManagementPermissions.RoomTypes.Create;
        UpdatePolicyName = HotelManagementPermissions.RoomTypes.Edit;
        DeletePolicyName = HotelManagementPermissions.RoomTypes.Delete;
    }
}
