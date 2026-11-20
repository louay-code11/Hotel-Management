using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HotelManagement.Rooms;

public interface IRoomTypeAppService : ICrudAppService<
    RoomTypeDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateRoomTypeDto>
{
}
