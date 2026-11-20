using System;
using Volo.Abp.Application.Services;

namespace HotelManagement.Guests;

public interface IGuestAppService : ICrudAppService<
    GuestDto,
    Guid,
    GetGuestListDto,
    CreateUpdateGuestDto>
{
}
