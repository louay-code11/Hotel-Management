using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HotelManagement.Services;

public interface IServiceAppService : ICrudAppService<
    ServiceDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateServiceDto>
{
}
