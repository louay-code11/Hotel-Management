using AutoMapper;
using HotelManagement.Guests;
using HotelManagement.Invoices;
using HotelManagement.Reservations;
using HotelManagement.Rooms;
using HotelManagement.Services;
using HotelManagement.Stays;

namespace HotelManagement;

public class HotelManagementApplicationAutoMapperProfile : Profile
{
    public HotelManagementApplicationAutoMapperProfile()
    {
        // RoomType
        CreateMap<RoomType, RoomTypeDto>();
        CreateMap<CreateUpdateRoomTypeDto, RoomType>();

        // Room
        CreateMap<Room, RoomDto>()
            .ForMember(d => d.RoomTypeName, opt => opt.MapFrom(s => s.RoomType != null ? s.RoomType.Name : null));
        CreateMap<CreateUpdateRoomDto, Room>();

        // RoomTask
        CreateMap<RoomTask, RoomTaskDto>()
            .ForMember(d => d.RoomNumber, opt => opt.MapFrom(s => s.Room != null ? s.Room.RoomNumber : null));
        CreateMap<CreateRoomTaskDto, RoomTask>();

        // Guest
        CreateMap<Guest, GuestDto>();
        CreateMap<CreateUpdateGuestDto, Guest>();

        // Reservation
        CreateMap<Reservation, ReservationDto>()
            .ForMember(d => d.GuestName, opt => opt.MapFrom(s => s.Guest != null ? $"{s.Guest.FirstName} {s.Guest.LastName}".Trim() : null));
        CreateMap<ReservationRoom, ReservationRoomDto>()
            .ForMember(d => d.RoomNumber, opt => opt.MapFrom(s => s.Room != null ? s.Room.RoomNumber : null));

        // Stay
        CreateMap<Stay, StayDto>()
            .ForMember(d => d.RoomNumber, opt => opt.MapFrom(s => s.Room != null ? s.Room.RoomNumber : null));
        CreateMap<StayGuest, StayGuestDto>()
            .ForMember(d => d.GuestName, opt => opt.MapFrom(s => s.Guest != null ? $"{s.Guest.FirstName} {s.Guest.LastName}".Trim() : null));

        // Service
        CreateMap<Service, ServiceDto>();
        CreateMap<CreateUpdateServiceDto, Service>();
        CreateMap<ServiceConsumption, ServiceConsumptionDto>()
            .ForMember(d => d.ServiceName, opt => opt.MapFrom(s => s.Service != null ? s.Service.Name : null));

        // Invoice & Payment
        CreateMap<Invoice, InvoiceDto>();
        CreateMap<Payment, PaymentDto>();
    }
}
