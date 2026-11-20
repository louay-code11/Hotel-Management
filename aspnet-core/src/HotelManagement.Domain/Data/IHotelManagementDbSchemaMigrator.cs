using System.Threading.Tasks;

namespace HotelManagement.Data;

public interface IHotelManagementDbSchemaMigrator
{
    Task MigrateAsync();
}
