using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Enums;
using HotelManagement.Rooms;
using HotelManagement.Services;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace HotelManagement.Data;

/// <summary>
/// Seeder pour la Base 2 (HotelManagement - HotelDbContext) :
/// Initialise les données de référence métier (RoomTypes, Rooms, Services)
/// de manière totalement idempotente (uniquement si les tables sont vides).
/// </summary>
public class HotelBusinessDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<RoomType, Guid> _roomTypeRepository;
    private readonly IRepository<Room, Guid> _roomRepository;
    private readonly IRepository<Service, Guid> _serviceRepository;
    private readonly IGuidGenerator _guidGenerator;

    public HotelBusinessDataSeedContributor(
        IRepository<RoomType, Guid> roomTypeRepository,
        IRepository<Room, Guid> roomRepository,
        IRepository<Service, Guid> serviceRepository,
        IGuidGenerator guidGenerator)
    {
        _roomTypeRepository = roomTypeRepository;
        _roomRepository = roomRepository;
        _serviceRepository = serviceRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedRoomTypesAndRoomsAsync();
        await SeedServicesAsync();
    }

    private async Task SeedRoomTypesAndRoomsAsync()
    {
        var roomTypes = new Dictionary<string, RoomType>();

        // 1. Insertion des 3 RoomTypes si la table est vide
        if (await _roomTypeRepository.GetCountAsync() == 0)
        {
            var singleType = new RoomType(
                _guidGenerator.Create(),
                "Single",
                maxCapacity: 1,
                baseNightlyRate: 80.00m,
                description: "Chambre individuelle avec lit simple confortable, espace de travail et salle d'eau."
            );
            await _roomTypeRepository.InsertAsync(singleType, autoSave: true);
            roomTypes["Single"] = singleType;

            var doubleType = new RoomType(
                _guidGenerator.Create(),
                "Double",
                maxCapacity: 2,
                baseNightlyRate: 120.00m,
                description: "Chambre double spacieuse avec lit Queen Size, idéale pour couples ou voyageurs d'affaires."
            );
            await _roomTypeRepository.InsertAsync(doubleType, autoSave: true);
            roomTypes["Double"] = doubleType;

            var suiteType = new RoomType(
                _guidGenerator.Create(),
                "Suite",
                maxCapacity: 4,
                baseNightlyRate: 250.00m,
                description: "Suite luxueuse avec salon indépendant, lit King Size, baignoire balnéo et vue panoramique."
            );
            await _roomTypeRepository.InsertAsync(suiteType, autoSave: true);
            roomTypes["Suite"] = suiteType;
        }
        else
        {
            var existingTypes = await _roomTypeRepository.GetListAsync();
            foreach (var rt in existingTypes)
            {
                roomTypes[rt.Name] = rt;
            }
        }

        // 2. Insertion des 6 Rooms (101 à 106) si la table est vide
        if (await _roomRepository.GetCountAsync() == 0)
        {
            var singleId = roomTypes.TryGetValue("Single", out var s) ? s.Id : (await _roomTypeRepository.GetListAsync())[0].Id;
            var doubleId = roomTypes.TryGetValue("Double", out var d) ? d.Id : (await _roomTypeRepository.GetListAsync())[0].Id;
            var suiteId = roomTypes.TryGetValue("Suite", out var su) ? su.Id : (await _roomTypeRepository.GetListAsync())[0].Id;

            var roomsToCreate = new List<Room>
            {
                new Room(_guidGenerator.Create(), singleId, "101", floor: 1, RoomStatus.Available),
                new Room(_guidGenerator.Create(), singleId, "102", floor: 1, RoomStatus.Available),
                new Room(_guidGenerator.Create(), doubleId, "103", floor: 1, RoomStatus.Available),
                new Room(_guidGenerator.Create(), doubleId, "104", floor: 1, RoomStatus.Available),
                new Room(_guidGenerator.Create(), suiteId, "105", floor: 1, RoomStatus.Available),
                new Room(_guidGenerator.Create(), suiteId, "106", floor: 1, RoomStatus.Available)
            };

            await _roomRepository.InsertManyAsync(roomsToCreate, autoSave: true);
        }
    }

    private async Task SeedServicesAsync()
    {
        // 3. Insertion des 4 Services si la table est vide
        if (await _serviceRepository.GetCountAsync() == 0)
        {
            var services = new List<Service>
            {
                new Service(
                    _guidGenerator.Create(),
                    "Petit-déjeuner buffet",
                    ServiceCategory.Food,
                    unitPrice: 15.00m,
                    taxRate: 10.00m,
                    isActive: true
                ),
                new Service(
                    _guidGenerator.Create(),
                    "Blanchisserie express",
                    ServiceCategory.Laundry,
                    unitPrice: 25.00m,
                    taxRate: 20.00m,
                    isActive: true
                ),
                new Service(
                    _guidGenerator.Create(),
                    "Accès Spa & Massage (1h)",
                    ServiceCategory.Spa,
                    unitPrice: 60.00m,
                    taxRate: 20.00m,
                    isActive: true
                ),
                new Service(
                    _guidGenerator.Create(),
                    "Consommation Minibar",
                    ServiceCategory.Minibar,
                    unitPrice: 10.00m,
                    taxRate: 20.00m,
                    isActive: true
                )
            };

            await _serviceRepository.InsertManyAsync(services, autoSave: true);
        }
    }
}
