using System;
using System.Collections.Generic;

namespace HotelManagement.Stays;

public class CheckInDto
{
    public List<Guid>? AdditionalGuestIds { get; set; }
}
