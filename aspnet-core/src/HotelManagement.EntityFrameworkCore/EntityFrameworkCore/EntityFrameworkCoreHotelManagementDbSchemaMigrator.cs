using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HotelManagement.Data;
using Volo.Abp.DependencyInjection;

namespace HotelManagement.EntityFrameworkCore;

public class EntityFrameworkCoreHotelManagementDbSchemaMigrator
    : IHotelManagementDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreHotelManagementDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* Migrate Base 1: ABP Default Database */
        await _serviceProvider
            .GetRequiredService<MainDbContext>()
            .Database
            .MigrateAsync();

        /* Migrate Base 2: Hotel Business Database */
        await _serviceProvider
            .GetRequiredService<HotelDbContext>()
            .Database
            .MigrateAsync();
    }
}
