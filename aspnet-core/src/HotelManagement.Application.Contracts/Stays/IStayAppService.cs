using System;
using System.Threading.Tasks;
using HotelManagement.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HotelManagement.Stays;

public interface IStayAppService : IApplicationService
{
    Task<StayDto> GetAsync(Guid id);
    Task<PagedResultDto<StayDto>> GetListAsync(GetStayListDto input);
    Task<StayDto> CheckOutAsync(Guid id);
    Task<ServiceConsumptionDto> AddConsumptionAsync(Guid id, AddConsumptionDto input);
    Task<StayGuestDto> AddGuestAsync(Guid id, AddStayGuestDto input);
}
