using HotelManagement.Samples;
using Xunit;

namespace HotelManagement.EntityFrameworkCore.Domains;

[Collection(HotelManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<HotelManagementEntityFrameworkCoreTestModule>
{

}
