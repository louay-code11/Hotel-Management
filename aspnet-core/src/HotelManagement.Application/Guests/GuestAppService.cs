using System;
using System.Linq;
using System.Threading.Tasks;
using HotelManagement.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace HotelManagement.Guests;

[Authorize(HotelManagementPermissions.Guests.Default)]
public class GuestAppService : CrudAppService<
    Guest,
    GuestDto,
    Guid,
    GetGuestListDto,
    CreateUpdateGuestDto>,
    IGuestAppService
{
    public GuestAppService(IRepository<Guest, Guid> repository)
        : base(repository)
    {
        GetPolicyName = HotelManagementPermissions.Guests.Default;
        GetListPolicyName = HotelManagementPermissions.Guests.Default;
        CreatePolicyName = HotelManagementPermissions.Guests.Create;
        UpdatePolicyName = HotelManagementPermissions.Guests.Edit;
        DeletePolicyName = HotelManagementPermissions.Guests.Delete;
    }

    protected override async Task<IQueryable<Guest>> CreateFilteredQueryAsync(GetGuestListDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(g =>
                g.FirstName.Contains(input.Filter) ||
                g.LastName.Contains(input.Filter) ||
                (g.Email != null && g.Email.Contains(input.Filter)) ||
                (g.Phone != null && g.Phone.Contains(input.Filter)) ||
                (g.IdDocumentNumber != null && g.IdDocumentNumber.Contains(input.Filter))
            );
        }

        return query;
    }
}
