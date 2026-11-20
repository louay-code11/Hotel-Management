using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Stays;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HotelManagement.Reservations;

public interface IReservationAppService : IApplicationService
{
    Task<ReservationDto> GetAsync(Guid id);
    Task<PagedResultDto<ReservationDto>> GetListAsync(GetReservationListDto input);
    Task<ReservationDto> CreateAsync(CreateReservationDto input);
    Task<ReservationDto> ConfirmAsync(Guid id);
    Task<ReservationDto> CancelAsync(Guid id);
    Task<List<AvailableRoomDto>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, Guid? roomTypeId = null);
    Task<List<StayDto>> CheckInAsync(Guid id, CheckInDto input);
}
