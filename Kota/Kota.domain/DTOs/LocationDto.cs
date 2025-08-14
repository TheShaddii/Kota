// Kota.Domain/DTOs/LocationDto.cs
namespace Kota.Domain.DTOs
{
    public class LocationDto
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; } = string.Empty;

        public int BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;

        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;

        public int? AreaId { get; set; }
        public string? AreaName { get; set; }

        public int StorageUnitId { get; set; }
        public string StorageUnitName { get; set; } = string.Empty;

        public int BinId { get; set; }
        public string BinName { get; set; } = string.Empty;

        public string FullPath => $"{SiteName} > {BuildingName} > {RoomName}" +
                                 (string.IsNullOrEmpty(AreaName) ? "" : $" > {AreaName}") +
                                 $" > {StorageUnitName} > {BinName}";
    }
}