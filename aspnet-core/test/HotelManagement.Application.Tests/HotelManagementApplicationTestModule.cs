using Volo.Abp.Modularity;

namespace HotelManagement;

[DependsOn(
    typeof(HotelManagementApplicationModule),
    typeof(HotelManagementDomainTestModule)
)]
public class HotelManagementApplicationTestModule : AbpModule
{

}
