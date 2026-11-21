using HotelManagement.Applications;
using Xunit;

namespace HotelManagement.EntityFrameworkCore.Applications;

[Collection(HotelManagementTestConsts.CollectionDefinitionName)]
public class EfCoreHotelBusinessAppServiceTests : HotelBusinessAppServiceTests<HotelManagementEntityFrameworkCoreTestModule>
{
}
