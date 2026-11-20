using HotelManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace HotelManagement.Permissions;

public class HotelManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(HotelManagementPermissions.GroupName, L("Permission:HotelManagement"));

        // RoomTypes
        var roomTypes = group.AddPermission(HotelManagementPermissions.RoomTypes.Default, L("Permission:RoomTypes"));
        roomTypes.AddChild(HotelManagementPermissions.RoomTypes.Create, L("Permission:Create"));
        roomTypes.AddChild(HotelManagementPermissions.RoomTypes.Edit, L("Permission:Edit"));
        roomTypes.AddChild(HotelManagementPermissions.RoomTypes.Delete, L("Permission:Delete"));

        // Rooms
        var rooms = group.AddPermission(HotelManagementPermissions.Rooms.Default, L("Permission:Rooms"));
        rooms.AddChild(HotelManagementPermissions.Rooms.Create, L("Permission:Create"));
        rooms.AddChild(HotelManagementPermissions.Rooms.Edit, L("Permission:Edit"));
        rooms.AddChild(HotelManagementPermissions.Rooms.Delete, L("Permission:Delete"));

        // Guests
        var guests = group.AddPermission(HotelManagementPermissions.Guests.Default, L("Permission:Guests"));
        guests.AddChild(HotelManagementPermissions.Guests.Create, L("Permission:Create"));
        guests.AddChild(HotelManagementPermissions.Guests.Edit, L("Permission:Edit"));
        guests.AddChild(HotelManagementPermissions.Guests.Delete, L("Permission:Delete"));

        // Reservations
        var reservations = group.AddPermission(HotelManagementPermissions.Reservations.Default, L("Permission:Reservations"));
        reservations.AddChild(HotelManagementPermissions.Reservations.Create, L("Permission:Create"));
        reservations.AddChild(HotelManagementPermissions.Reservations.Edit, L("Permission:Edit"));
        reservations.AddChild(HotelManagementPermissions.Reservations.Delete, L("Permission:Delete"));
        reservations.AddChild(HotelManagementPermissions.Reservations.CheckIn, L("Permission:CheckIn"));
        reservations.AddChild(HotelManagementPermissions.Reservations.Cancel, L("Permission:Cancel"));

        // Stays
        var stays = group.AddPermission(HotelManagementPermissions.Stays.Default, L("Permission:Stays"));
        stays.AddChild(HotelManagementPermissions.Stays.Create, L("Permission:Create"));
        stays.AddChild(HotelManagementPermissions.Stays.Edit, L("Permission:Edit"));
        stays.AddChild(HotelManagementPermissions.Stays.Delete, L("Permission:Delete"));
        stays.AddChild(HotelManagementPermissions.Stays.CheckOut, L("Permission:CheckOut"));
        stays.AddChild(HotelManagementPermissions.Stays.AddConsumption, L("Permission:AddConsumption"));

        // Services
        var services = group.AddPermission(HotelManagementPermissions.Services.Default, L("Permission:Services"));
        services.AddChild(HotelManagementPermissions.Services.Create, L("Permission:Create"));
        services.AddChild(HotelManagementPermissions.Services.Edit, L("Permission:Edit"));
        services.AddChild(HotelManagementPermissions.Services.Delete, L("Permission:Delete"));

        // Invoices
        var invoices = group.AddPermission(HotelManagementPermissions.Invoices.Default, L("Permission:Invoices"));
        invoices.AddChild(HotelManagementPermissions.Invoices.Create, L("Permission:Create"));
        invoices.AddChild(HotelManagementPermissions.Invoices.Edit, L("Permission:Edit"));
        invoices.AddChild(HotelManagementPermissions.Invoices.Delete, L("Permission:Delete"));
        invoices.AddChild(HotelManagementPermissions.Invoices.Issue, L("Permission:Issue"));
        invoices.AddChild(HotelManagementPermissions.Invoices.Void, L("Permission:Void"));
        invoices.AddChild(HotelManagementPermissions.Invoices.RecordPayment, L("Permission:RecordPayment"));

        // RoomTasks
        var tasks = group.AddPermission(HotelManagementPermissions.RoomTasks.Default, L("Permission:RoomTasks"));
        tasks.AddChild(HotelManagementPermissions.RoomTasks.Create, L("Permission:Create"));
        tasks.AddChild(HotelManagementPermissions.RoomTasks.Edit, L("Permission:Edit"));
        tasks.AddChild(HotelManagementPermissions.RoomTasks.Delete, L("Permission:Delete"));
        tasks.AddChild(HotelManagementPermissions.RoomTasks.Assign, L("Permission:Assign"));
        tasks.AddChild(HotelManagementPermissions.RoomTasks.Complete, L("Permission:Complete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HotelManagementResource>(name);
    }
}
