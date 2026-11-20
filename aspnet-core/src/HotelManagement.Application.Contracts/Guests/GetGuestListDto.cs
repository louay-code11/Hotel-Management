using Volo.Abp.Application.Dtos;

namespace HotelManagement.Guests;

public class GetGuestListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
