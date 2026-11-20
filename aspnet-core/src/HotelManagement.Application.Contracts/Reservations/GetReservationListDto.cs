using System;
using HotelManagement.Enums;
using Volo.Abp.Application.Dtos;

namespace HotelManagement.Reservations;

public class GetReservationListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public ReservationStatus? Status { get; set; }
    public Guid? GuestId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
