// Kota.Services/Locations/ILocationService.cs
using Kota.Domain.Entities;

namespace Kota.Services.Locations
{
    public interface ILocationService
    {
        Task<IEnumerable<Site>> GetAllSitesAsync();
        Task<IEnumerable<Building>> GetBuildingsBySiteAsync(int siteId);
        Task<IEnumerable<Room>> GetRoomsByBuildingAsync(int buildingId);
        Task<IEnumerable<Area>> GetAreasByRoomAsync(int roomId);
        Task<IEnumerable<StorageUnit>> GetStorageUnitsByRoomAsync(int roomId, int? areaId = null);
        Task<IEnumerable<Bin>> GetBinsByStorageUnitAsync(int storageUnitId);

        Task<Site> CreateSiteAsync(Site site, int userId);
        Task<Building> CreateBuildingAsync(Building building, int userId);
        Task<Room> CreateRoomAsync(Room room, int userId);
        Task<Area> CreateAreaAsync(Area area, int userId);
        Task<StorageUnit> CreateStorageUnitAsync(StorageUnit storageUnit, int userId);
        Task<Bin> CreateBinAsync(Bin bin, int userId);

        Task ValidateLocationHierarchyAsync(int binId);
    }
}