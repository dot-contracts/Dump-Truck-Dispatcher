using Newtonsoft.Json;

namespace DispatcherWeb.Fulcrum.Dto
{
    public class VehicleDto
    {
        public string Id { get; set; }

        public string Uiid { get; set; }

        public string CustomerId { get; set; }

        public string Trailer { get; set; }

        public string DtId { get; set; }

        public bool Inactive { get; set; }

        public string Registration { get; set; }

        public string VehicleTypeId { get; set; }

        [JsonIgnore]
        public AssetType AssetType { get; set; }

        [JsonIgnore]
        public string VehicleCategoryName { get; set; }
    }

}
