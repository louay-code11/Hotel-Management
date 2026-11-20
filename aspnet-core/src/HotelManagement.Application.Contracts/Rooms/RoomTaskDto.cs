using System;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Rooms;

public class RoomTaskDto : FullAuditedEntityDto<Guid>
{
    public Guid RoomId { get; set; }
    public string? RoomNumber { get; set; }

    /// <summary>
    /// Référence vers l'utilisateur assigné (Base ABP Identity)
    /// </summary>
    public Guid? AssignedUserId { get; set; }

    /// <summary>
    /// Résolu en mémoire côté backend sans jointure SQL cross-database
    /// </summary>
    public string? AssignedUserName { get; set; }

    public RoomTaskType TaskType { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public RoomTaskStatus Status { get; set; }
}
