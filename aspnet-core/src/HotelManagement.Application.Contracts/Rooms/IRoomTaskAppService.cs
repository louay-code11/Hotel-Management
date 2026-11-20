using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HotelManagement.Rooms;

public interface IRoomTaskAppService : IApplicationService
{
    Task<RoomTaskDto> GetAsync(Guid id);
    Task<PagedResultDto<RoomTaskDto>> GetListAsync(GetRoomTaskListDto input);
    Task<RoomTaskDto> CreateAsync(CreateRoomTaskDto input);
    Task<RoomTaskDto> AssignAsync(Guid id, Guid? userId);
    Task<RoomTaskDto> CompleteAsync(Guid id);
}
