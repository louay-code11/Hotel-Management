using HotelManagement.Samples;
using Xunit;

namespace HotelManagement.EntityFrameworkCore.Applications;

[Collection(HotelManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<HotelManagementEntityFrameworkCoreTestModule>
{

}
