using System;
using System.Collections.Generic;
using System.Text;
using HotelManagement.Localization;
using Volo.Abp.Application.Services;

namespace HotelManagement;

/* Inherit your application services from this class.
 */
public abstract class HotelManagementAppService : ApplicationService
{
    protected HotelManagementAppService()
    {
        LocalizationResource = typeof(HotelManagementResource);
    }
}
