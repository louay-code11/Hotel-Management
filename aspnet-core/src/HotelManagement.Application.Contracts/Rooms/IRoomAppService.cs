using System;
using Volo.Abp.Application.Services;

namespace HotelManagement.Rooms;

public interface IRoomAppService : ICrudAppService<
    RoomDto,
    Guid,
    GetRoomListDto,
    CreateUpdateRoomDto>
{
}
