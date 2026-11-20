using Microsoft.Extensions.Localization;
using HotelManagement.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace HotelManagement;

[Dependency(ReplaceServices = true)]
public class HotelManagementBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<HotelManagementResource> _localizer;

    public HotelManagementBrandingProvider(IStringLocalizer<HotelManagementResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
