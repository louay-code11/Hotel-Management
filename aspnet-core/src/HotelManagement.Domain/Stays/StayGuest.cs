using System;
using HotelManagement.Guests;
using Volo.Abp.Domain.Entities.Auditing;

namespace HotelManagement.Stays;

public class StayGuest : FullAuditedEntity<Guid>
{
    public Guid StayId { get; set; }
    public Guid GuestId { get; set; }
    public bool IsPrimaryGuest { get; set; }

    public virtual Stay? Stay { get; set; }
    public virtual Guest? Guest { get; set; }

    protected StayGuest()
    {
    }

    public StayGuest(Guid id, Guid stayId, Guid guestId, bool isPrimaryGuest = false)
        : base(id)
    {
        StayId = stayId;
        GuestId = guestId;
        IsPrimaryGuest = isPrimaryGuest;
    }
}
