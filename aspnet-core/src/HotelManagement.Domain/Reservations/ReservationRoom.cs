using System;
using HotelManagement.Rooms;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Reservations;

public class ReservationRoom : FullAuditedEntity<Guid>
{
    public Guid ReservationId { get; set; }
    public Guid RoomId { get; set; }
    public decimal AgreedNightlyRate { get; set; }
    public int NightsCount { get; set; }

    public virtual Reservation? Reservation { get; set; }
    public virtual Room? Room { get; set; }

    protected ReservationRoom()
    {
    }

    public ReservationRoom(Guid id, Guid reservationId, Guid roomId, decimal agreedNightlyRate, int nightsCount)
        : base(id)
    {
        ReservationId = reservationId;
        RoomId = roomId;
        AgreedNightlyRate = agreedNightlyRate;
        NightsCount = nightsCount;
    }
}
