using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace HotelManagement.EntityFrameworkCore;

[DependsOn(
    typeof(HotelManagementDomainModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule)
    )]
public class HotelManagementEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        // Évite les erreurs "Cannot write DateTime with Kind=Unspecified to PostgreSQL"
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        HotelManagementEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Configuration de l'horloge ABP en UTC
        Configure<AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Utc;
        });

        // Base 1: Standard ABP DbContext (Default)
        context.Services.AddAbpDbContext<MainDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        // Base 2: Business Hotel DbContext (HotelManagement)
        context.Services.AddAbpDbContext<HotelDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql();
        });
    }
}
