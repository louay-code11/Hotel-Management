using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagement.Enums;
using HotelManagement.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace HotelManagement.Rooms;

[Authorize(HotelManagementPermissions.RoomTasks.Default)]
public class RoomTaskAppService : ApplicationService, IRoomTaskAppService
{
    private readonly IRepository<RoomTask, Guid> _taskRepository;
    private readonly IRepository<Room, Guid> _roomRepository;

    /// <summary>
    /// Référentiel des utilisateurs ABP (Base 1 : Default / MainDbContext).
    /// Aucune jointure SQL cross-database n'est effectuée. La résolution des noms s'effectue en mémoire.
    /// </summary>
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public RoomTaskAppService(
        IRepository<RoomTask, Guid> taskRepository,
        IRepository<Room, Guid> roomRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _taskRepository = taskRepository;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
    }

    public async Task<RoomTaskDto> GetAsync(Guid id)
    {
        var task = await _taskRepository.GetAsync(id);
        var dto = ObjectMapper.Map<RoomTask, RoomTaskDto>(task);

        var room = await _roomRepository.FindAsync(task.RoomId);
        dto.RoomNumber = room?.RoomNumber;

        // Résolution cross-database en mémoire
        if (task.AssignedUserId.HasValue)
        {
            var user = await _userRepository.FindAsync(task.AssignedUserId.Value);
            if (user != null)
            {
                dto.AssignedUserName = string.IsNullOrWhiteSpace(user.Name) && string.IsNullOrWhiteSpace(user.Surname)
                    ? user.UserName
                    : $"{user.Name} {user.Surname}".Trim();
            }
        }

        return dto;
    }

    public async Task<PagedResultDto<RoomTaskDto>> GetListAsync(GetRoomTaskListDto input)
    {
        var query = await _taskRepository.GetQueryableAsync();

        if (input.RoomId.HasValue)
        {
            query = query.Where(t => t.RoomId == input.RoomId.Value);
        }

        if (input.AssignedUserId.HasValue)
        {
            query = query.Where(t => t.AssignedUserId == input.AssignedUserId.Value);
        }

        if (input.TaskType.HasValue)
        {
            query = query.Where(t => t.TaskType == input.TaskType.Value);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(t => t.Status == input.Status.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderByDescending(t => t.ScheduledDate)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var tasks = await AsyncExecuter.ToListAsync(query);
        var dtos = ObjectMapper.Map<List<RoomTask>, List<RoomTaskDto>>(tasks);

        // 1. Résolution des numéros de chambres (Base 2 : HotelDbContext)
        var roomIds = tasks.Select(t => t.RoomId).Distinct().ToList();
        var rooms = await _roomRepository.GetListAsync(r => roomIds.Contains(r.Id));
        var roomDict = rooms.ToDictionary(r => r.Id, r => r.RoomNumber);

        // 2. RÉSOLUTION CROSS-DATABASE (Base 1 : Identity Users)
        // Extraire les AssignedUserId distincts non nuls et interroger en UNE SEULE requête
        var assignedUserIds = tasks
            .Where(t => t.AssignedUserId.HasValue)
            .Select(t => t.AssignedUserId!.Value)
            .Distinct()
            .ToList();

        var userDict = new Dictionary<Guid, string>();
        if (assignedUserIds.Any())
        {
            var userQuery = await _userRepository.GetQueryableAsync();
            var users = await AsyncExecuter.ToListAsync(userQuery.Where(u => assignedUserIds.Contains(u.Id)));
            userDict = users.ToDictionary(
                u => u.Id,
                u => string.IsNullOrWhiteSpace(u.Name) && string.IsNullOrWhiteSpace(u.Surname)
                    ? u.UserName
                    : $"{u.Name} {u.Surname}".Trim()
            );
        }

        // 3. Remplissage des DTOs en mémoire
        foreach (var dto in dtos)
        {
            if (roomDict.TryGetValue(dto.RoomId, out var roomNum))
            {
                dto.RoomNumber = roomNum;
            }

            if (dto.AssignedUserId.HasValue && userDict.TryGetValue(dto.AssignedUserId.Value, out var userName))
            {
                dto.AssignedUserName = userName;
            }
        }

        return new PagedResultDto<RoomTaskDto>(totalCount, dtos);
    }

    [Authorize(HotelManagementPermissions.RoomTasks.Create)]
    public async Task<RoomTaskDto> CreateAsync(CreateRoomTaskDto input)
    {
        var task = new RoomTask(
            GuidGenerator.Create(),
            input.RoomId,
            input.TaskType,
            input.ScheduledDate,
            input.AssignedUserId,
            RoomTaskStatus.Pending
        );

        await _taskRepository.InsertAsync(task, autoSave: true);
        return await GetAsync(task.Id);
    }

    [Authorize(HotelManagementPermissions.RoomTasks.Assign)]
    public async Task<RoomTaskDto> AssignAsync(Guid id, Guid? userId)
    {
        var task = await _taskRepository.GetAsync(id);

        if (userId.HasValue)
        {
            var userExists = await _userRepository.AnyAsync(u => u.Id == userId.Value);
            if (!userExists)
            {
                throw new BusinessException(
                    "HotelManagement:UserNotFound",
                    $"Assigned user with ID '{userId}' was not found in Identity database."
                );
            }
        }

        task.AssignedUserId = userId;
        if (userId.HasValue && task.Status == RoomTaskStatus.Pending)
        {
            task.Status = RoomTaskStatus.InProgress;
        }

        await _taskRepository.UpdateAsync(task, autoSave: true);
        return await GetAsync(id);
    }

    [Authorize(HotelManagementPermissions.RoomTasks.Complete)]
    public async Task<RoomTaskDto> CompleteAsync(Guid id)
    {
        var task = await _taskRepository.GetAsync(id);
        task.Status = RoomTaskStatus.Done;
        task.CompletedAt = Clock.Now;
        await _taskRepository.UpdateAsync(task, autoSave: true);

        // Si c'était un ménage (Cleaning), repasser la chambre à Available si elle était en Cleaning
        if (task.TaskType == RoomTaskType.Cleaning)
        {
            var room = await _roomRepository.FindAsync(task.RoomId);
            if (room != null && room.Status == RoomStatus.Cleaning)
            {
                room.Status = RoomStatus.Available;
                await _roomRepository.UpdateAsync(room, autoSave: true);
            }
        }

        return await GetAsync(id);
    }
}
