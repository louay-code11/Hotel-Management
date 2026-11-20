using Volo.Abp.Modularity;

namespace HotelManagement;

[DependsOn(
    typeof(HotelManagementDomainModule),
    typeof(HotelManagementTestBaseModule)
)]
public class HotelManagementDomainTestModule : AbpModule
{

}
