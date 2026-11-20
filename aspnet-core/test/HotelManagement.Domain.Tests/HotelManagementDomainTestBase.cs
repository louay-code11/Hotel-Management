using Volo.Abp.Modularity;

namespace HotelManagement;

/* Inherit from this class for your domain layer tests. */
public abstract class HotelManagementDomainTestBase<TStartupModule> : HotelManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
