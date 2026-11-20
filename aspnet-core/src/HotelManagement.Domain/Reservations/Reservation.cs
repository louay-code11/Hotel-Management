using System;
using System.Collections.Generic;
using HotelManagement.Enums;
using HotelManagement.Guests;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Reservations;

public class Reservation : FullAuditedAggregateRoot<Guid>
{
    public string ReservationCode { get; set; } = string.Empty;
    public Guid GuestId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int AdultsCount { get; set; }
    public int ChildrenCount { get; set; }
    public ReservationStatus Status { get; set; }
    public string? Notes { get; set; }

    public virtual Guest? Guest { get; set; }
    public virtual ICollection<ReservationRoom> Rooms { get; protected set; } = new List<ReservationRoom>();

    protected Reservation()
    {
    }

    public Reservation(
        Guid id,
        string reservationCode,
        Guid guestId,
        DateTime checkInDate,
        DateTime checkOutDate,
        int adultsCount,
        int childrenCount = 0,
        ReservationStatus status = ReservationStatus.Pending,
        string? notes = null)
        : base(id)
    {
        ReservationCode = Check.NotNullOrWhiteSpace(reservationCode, nameof(reservationCode), maxLength: 32);
        GuestId = guestId;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        AdultsCount = adultsCount;
        ChildrenCount = childrenCount;
        Status = status;
        Notes = notes;
    }
}
