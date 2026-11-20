using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace HotelManagement.Data;

/* This is used if database provider does't define
 * IHotelManagementDbSchemaMigrator implementation.
 */
public class NullHotelManagementDbSchemaMigrator : IHotelManagementDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
