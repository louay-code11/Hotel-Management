namespace HotelManagement.Permissions;

public static class HotelManagementPermissions
{
    public const string GroupName = "HotelManagement";

    public static class Rooms
    {
        public const string Default = GroupName + ".Rooms";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class RoomTypes
    {
        public const string Default = GroupName + ".RoomTypes";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Guests
    {
        public const string Default = GroupName + ".Guests";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Reservations
    {
        public const string Default = GroupName + ".Reservations";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string CheckIn = Default + ".CheckIn";
        public const string Cancel = Default + ".Cancel";
    }

    public static class Stays
    {
        public const string Default = GroupName + ".Stays";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string CheckOut = Default + ".CheckOut";
        public const string AddConsumption = Default + ".AddConsumption";
    }

    public static class Services
    {
        public const string Default = GroupName + ".Services";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Invoices
    {
        public const string Default = GroupName + ".Invoices";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Issue = Default + ".Issue";
        public const string Void = Default + ".Void";
        public const string RecordPayment = Default + ".RecordPayment";
    }

    public static class RoomTasks
    {
        public const string Default = GroupName + ".RoomTasks";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Assign = Default + ".Assign";
        public const string Complete = Default + ".Complete";
    }
}
