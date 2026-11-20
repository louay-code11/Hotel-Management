using HotelManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace HotelManagement.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class HotelManagementController : AbpControllerBase
{
    protected HotelManagementController()
    {
        LocalizationResource = typeof(HotelManagementResource);
    }
}
