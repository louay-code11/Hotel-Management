using System;
using System.Collections.Generic;
using HotelManagement.Enums;
using HotelManagement.Invoices;
using HotelManagement.Reservations;
using HotelManagement.Rooms;
using HotelManagement.Services;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Stays;

public class Stay : FullAuditedAggregateRoot<Guid>
{
    public Guid? ReservationId { get; set; } // Nullable pour gérer les walk-ins
    public Guid RoomId { get; set; }
    public DateTime ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }
    public StayStatus Status { get; set; }

    public virtual Reservation? Reservation { get; set; }
    public virtual Room? Room { get; set; }
    public virtual ICollection<StayGuest> Guests { get; protected set; } = new List<StayGuest>();
    public virtual ICollection<ServiceConsumption> Consumptions { get; protected set; } = new List<ServiceConsumption>();
    public virtual ICollection<Invoice> Invoices { get; protected set; } = new List<Invoice>();

    protected Stay()
    {
    }

    public Stay(Guid id, Guid? reservationId, Guid roomId, DateTime actualCheckIn, StayStatus status = StayStatus.Active)
        : base(id)
    {
        ReservationId = reservationId;
        RoomId = roomId;
        ActualCheckIn = actualCheckIn;
        Status = status;
    }
}
