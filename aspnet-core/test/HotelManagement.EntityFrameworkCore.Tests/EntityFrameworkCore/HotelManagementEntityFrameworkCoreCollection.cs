using Xunit;

namespace HotelManagement.EntityFrameworkCore;

[CollectionDefinition(HotelManagementTestConsts.CollectionDefinitionName)]
public class HotelManagementEntityFrameworkCoreCollection : ICollectionFixture<HotelManagementEntityFrameworkCoreFixture>
{

}
