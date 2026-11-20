namespace HotelManagement;

public static class HotelManagementDomainErrorCodes
{
    public const string RoomNotAvailable = "HotelManagement:RoomNotAvailable";
    public const string RoomNotFound = "HotelManagement:RoomNotFound";
    public const string GuestNotFound = "HotelManagement:GuestNotFound";
    public const string ReservationNotFound = "HotelManagement:ReservationNotFound";
    public const string ReservationNotConfirmed = "HotelManagement:ReservationNotConfirmed";
    public const string ReservationAlreadyCheckedIn = "HotelManagement:ReservationAlreadyCheckedIn";
    public const string StayNotFound = "HotelManagement:StayNotFound";
    public const string StayNotActive = "HotelManagement:StayNotActive";
    public const string InvoiceImmutable = "HotelManagement:InvoiceImmutable";
    public const string InvoiceNotFound = "HotelManagement:InvoiceNotFound";
    public const string InvoiceVoid = "HotelManagement:InvoiceVoid";
    public const string InvalidDateRange = "HotelManagement:InvalidDateRange";
}
