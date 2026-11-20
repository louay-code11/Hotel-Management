using HotelManagement.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace HotelManagement.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(HotelManagementEntityFrameworkCoreModule),
    typeof(HotelManagementApplicationContractsModule)
    )]
public class HotelManagementDbMigratorModule : AbpModule
{
}
