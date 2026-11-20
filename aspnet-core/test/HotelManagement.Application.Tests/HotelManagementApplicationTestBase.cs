using Volo.Abp.Modularity;

namespace HotelManagement;

public abstract class HotelManagementApplicationTestBase<TStartupModule> : HotelManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
