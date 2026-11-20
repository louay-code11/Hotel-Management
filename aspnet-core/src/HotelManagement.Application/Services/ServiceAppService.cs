using System;
using HotelManagement.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace HotelManagement.Services;

public class ServiceAppService : CrudAppService<
    Service,
    ServiceDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateServiceDto>,
    IServiceAppService
{
    public ServiceAppService(IRepository<Service, Guid> repository)
        : base(repository)
    {
        GetPolicyName = HotelManagementPermissions.Services.Default;
        GetListPolicyName = HotelManagementPermissions.Services.Default;
        CreatePolicyName = HotelManagementPermissions.Services.Create;
        UpdatePolicyName = HotelManagementPermissions.Services.Edit;
        DeletePolicyName = HotelManagementPermissions.Services.Delete;
    }
}
