using Microsoft.EntityFrameworkCore;
using HotelManagement.Guests;
using HotelManagement.Invoices;
using HotelManagement.Reservations;
using HotelManagement.Rooms;
using HotelManagement.Services;
using HotelManagement.Stays;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace HotelManagement.EntityFrameworkCore;

/* Base 2 (HotelManagement) : Base métier dédiée contenant UNIQUEMENT les entités hôtelières.
 * Aucune table ABP Identity / Roles ne doit être configurée ou migrée dans ce contexte.
 */
[ConnectionStringName("HotelManagement")]
public class HotelDbContext : AbpDbContext<HotelDbContext>
{
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Guest> Guests { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReservationRoom> ReservationRooms { get; set; }
    public DbSet<Stay> Stays { get; set; }
    public DbSet<StayGuest> StayGuests { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<ServiceConsumption> ServiceConsumptions { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<RoomTask> RoomTasks { get; set; }

    public HotelDbContext(DbContextOptions<HotelDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. RoomType
        builder.Entity<RoomType>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "RoomTypes", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.BaseNightlyRate).HasPrecision(18, 2);
            b.Property(x => x.Description).HasMaxLength(1024);
        });

        // 2. Room
        builder.Entity<Room>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "Rooms", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.RoomNumber).IsRequired().HasMaxLength(32);
            b.HasIndex(x => x.RoomNumber).IsUnique();
            b.HasOne(x => x.RoomType).WithMany().HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        // 3. Guest
        builder.Entity<Guest>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "Guests", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FirstName).IsRequired().HasMaxLength(64);
            b.Property(x => x.LastName).IsRequired().HasMaxLength(64);
            b.Property(x => x.Email).HasMaxLength(128);
            b.Property(x => x.Phone).HasMaxLength(32);
            b.Property(x => x.IdDocumentNumber).HasMaxLength(64);
            b.Property(x => x.Nationality).HasMaxLength(64);
        });

        // 4. Reservation
        builder.Entity<Reservation>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "Reservations", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ReservationCode).IsRequired().HasMaxLength(32);
            b.HasIndex(x => x.ReservationCode).IsUnique();
            b.Property(x => x.Notes).HasMaxLength(2048);
            b.HasOne(x => x.Guest).WithMany().HasForeignKey(x => x.GuestId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Rooms).WithOne(x => x.Reservation).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
        });

        // 5. ReservationRoom
        builder.Entity<ReservationRoom>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "ReservationRooms", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AgreedNightlyRate).HasPrecision(18, 2);
            b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        });

        // 6. Stay
        builder.Entity<Stay>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "Stays", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne(x => x.Reservation).WithMany().HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Guests).WithOne(x => x.Stay).HasForeignKey(x => x.StayId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Consumptions).WithOne(x => x.Stay).HasForeignKey(x => x.StayId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Invoices).WithOne(x => x.Stay).HasForeignKey(x => x.StayId).OnDelete(DeleteBehavior.Restrict);
        });

        // 7. StayGuest
        builder.Entity<StayGuest>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "StayGuests", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne(x => x.Guest).WithMany().HasForeignKey(x => x.GuestId).OnDelete(DeleteBehavior.Restrict);
        });

        // 8. Service
        builder.Entity<Service>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "Services", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);
            b.Property(x => x.TaxRate).HasPrecision(18, 2);
        });

        // 9. ServiceConsumption
        builder.Entity<ServiceConsumption>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "ServiceConsumptions", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.BilledUnitPrice).HasPrecision(18, 2);
            b.Property(x => x.TaxRate).HasPrecision(18, 2);
            b.HasOne(x => x.Service).WithMany().HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });

        // 10. Invoice
        builder.Entity<Invoice>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "Invoices", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(64);
            b.HasIndex(x => x.InvoiceNumber).IsUnique();
            b.Property(x => x.SubtotalAmount).HasPrecision(18, 2);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2);
            b.Property(x => x.TotalAmount).HasPrecision(18, 2);
            b.HasMany(x => x.Payments).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        });

        // 11. Payment
        builder.Entity<Payment>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "Payments", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.TransactionRef).HasMaxLength(128);
        });

        // 12. RoomTask
        builder.Entity<RoomTask>(b =>
        {
            b.ToTable(HotelManagementConsts.DbTablePrefix + "RoomTasks", HotelManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);

            // CONTRAINTE ARCHITECTURALE CRITIQUE :
            // AssignedUserId est une référence logique vers la table AbpUsers de la Base 1 (Default).
            // Aucune contrainte de clé étrangère SQL (HasForeignKey/HasOne) ne doit être créée ici.
            b.Property(x => x.AssignedUserId).IsRequired(false);
            b.HasIndex(x => x.AssignedUserId);
        });
    }
}
