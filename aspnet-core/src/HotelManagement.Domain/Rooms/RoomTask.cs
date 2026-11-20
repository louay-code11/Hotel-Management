using System;
using HotelManagement.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Rooms;

public class RoomTask : FullAuditedAggregateRoot<Guid>
{
    public Guid RoomId { get; set; }

    /// <summary>
    /// Référence logique vers AbpUsers.Id dans la Base 1 (Default).
    /// ATTENTION ARCHITECTURE : Ne jamais créer de contrainte de clé étrangère SQL pour cette propriété,
    /// car les utilisateurs résident dans une base de données PostgreSQL séparée.
    /// </summary>
    public Guid? AssignedUserId { get; set; }

    public RoomTaskType TaskType { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public RoomTaskStatus Status { get; set; }

    public virtual Room? Room { get; set; }

    protected RoomTask()
    {
    }

    public RoomTask(
        Guid id,
        Guid roomId,
        RoomTaskType taskType,
        DateTime scheduledDate,
        Guid? assignedUserId = null,
        RoomTaskStatus status = RoomTaskStatus.Pending,
        DateTime? completedAt = null)
        : base(id)
    {
        RoomId = roomId;
        TaskType = taskType;
        ScheduledDate = scheduledDate;
        AssignedUserId = assignedUserId;
        Status = status;
        CompletedAt = completedAt;
    }
}
