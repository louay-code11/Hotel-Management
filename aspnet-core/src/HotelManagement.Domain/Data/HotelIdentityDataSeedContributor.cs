using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace HotelManagement.Data;

/// <summary>
/// Seeder pour la Base 1 (Default - ABP Identity) :
/// Initialise les 3 rôles métier et leurs permissions dans la base Identity.
/// </summary>
public class HotelIdentityDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IdentityRoleManager _roleManager;
    private readonly IPermissionManager _permissionManager;
    private readonly IGuidGenerator _guidGenerator;

    public HotelIdentityDataSeedContributor(
        IdentityRoleManager roleManager,
        IPermissionManager permissionManager,
        IGuidGenerator guidGenerator)
    {
        _roleManager = roleManager;
        _permissionManager = permissionManager;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedRolesAndPermissionsAsync(context.TenantId);
    }

    private async Task SeedRolesAndPermissionsAsync(Guid? tenantId)
    {
        // 1. HotelManager : Toutes les permissions HotelManagement.*
        await CreateRoleIfNotExistsAsync("HotelManager", tenantId, new[]
        {
            "HotelManagement.Rooms", "HotelManagement.Rooms.Create", "HotelManagement.Rooms.Edit", "HotelManagement.Rooms.Delete",
            "HotelManagement.RoomTypes", "HotelManagement.RoomTypes.Create", "HotelManagement.RoomTypes.Edit", "HotelManagement.RoomTypes.Delete",
            "HotelManagement.Guests", "HotelManagement.Guests.Create", "HotelManagement.Guests.Edit", "HotelManagement.Guests.Delete",
            "HotelManagement.Reservations", "HotelManagement.Reservations.Create", "HotelManagement.Reservations.Edit", "HotelManagement.Reservations.Delete", "HotelManagement.Reservations.CheckIn", "HotelManagement.Reservations.Cancel",
            "HotelManagement.Stays", "HotelManagement.Stays.Create", "HotelManagement.Stays.Edit", "HotelManagement.Stays.Delete", "HotelManagement.Stays.CheckOut", "HotelManagement.Stays.AddConsumption",
            "HotelManagement.Services", "HotelManagement.Services.Create", "HotelManagement.Services.Edit", "HotelManagement.Services.Delete",
            "HotelManagement.Invoices", "HotelManagement.Invoices.Create", "HotelManagement.Invoices.Edit", "HotelManagement.Invoices.Delete", "HotelManagement.Invoices.Issue", "HotelManagement.Invoices.Void", "HotelManagement.Invoices.RecordPayment",
            "HotelManagement.RoomTasks", "HotelManagement.RoomTasks.Create", "HotelManagement.RoomTasks.Edit", "HotelManagement.RoomTasks.Delete", "HotelManagement.RoomTasks.Assign", "HotelManagement.RoomTasks.Complete"
        });

        // 2. Receptionist : Rooms (lecture), Guests, Reservations, Stays, Invoices, RoomTasks (lecture/création)
        await CreateRoleIfNotExistsAsync("Receptionist", tenantId, new[]
        {
            "HotelManagement.Rooms",
            "HotelManagement.RoomTypes",
            "HotelManagement.Guests", "HotelManagement.Guests.Create", "HotelManagement.Guests.Edit", "HotelManagement.Guests.Delete",
            "HotelManagement.Reservations", "HotelManagement.Reservations.Create", "HotelManagement.Reservations.Edit", "HotelManagement.Reservations.Delete", "HotelManagement.Reservations.CheckIn", "HotelManagement.Reservations.Cancel",
            "HotelManagement.Stays", "HotelManagement.Stays.Create", "HotelManagement.Stays.Edit", "HotelManagement.Stays.Delete", "HotelManagement.Stays.CheckOut", "HotelManagement.Stays.AddConsumption",
            "HotelManagement.Services",
            "HotelManagement.Invoices", "HotelManagement.Invoices.Create", "HotelManagement.Invoices.Edit", "HotelManagement.Invoices.Issue", "HotelManagement.Invoices.RecordPayment",
            "HotelManagement.RoomTasks", "HotelManagement.RoomTasks.Create"
        });

        // 3. Housekeeper : Rooms (lecture + mise à jour statut), RoomTasks (lecture + Complete)
        await CreateRoleIfNotExistsAsync("Housekeeper", tenantId, new[]
        {
            "HotelManagement.Rooms", "HotelManagement.Rooms.Edit",
            "HotelManagement.RoomTasks", "HotelManagement.RoomTasks.Complete"
        });
    }

    private async Task CreateRoleIfNotExistsAsync(string roleName, Guid? tenantId, IEnumerable<string> permissions)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            role = new IdentityRole(_guidGenerator.Create(), roleName, tenantId)
            {
                IsStatic = false,
                IsPublic = true
            };
            (await _roleManager.CreateAsync(role)).CheckErrors();
        }

        foreach (var permission in permissions)
        {
            await _permissionManager.SetForRoleAsync(role.Name, permission, isGranted: true);
        }
    }
}
