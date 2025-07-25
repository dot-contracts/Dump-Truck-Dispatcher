using DispatcherWeb.Dto;

namespace DispatcherWeb.Trucks.Dto
{
    public class GetTrucksSelectListInput : GetSelectListInput
    {
        public bool InServiceOnly { get; set; }

        public bool ActiveOnly { get; set; }

        public bool AllOffices { get; set; }

        public int? OfficeId { get; set; }

        public bool ExcludeTrailers { get; set; }

        public bool ExcludeInternalTrucks { get; set; }

        //public bool ExcludeLeaseHaulers { get; set; }

        public bool IncludeLeaseHaulerTrucks { get; set; }

        public int? OrderLineId { get; set; }

        public AssetType? AssetType { get; set; }

        public bool? CanPullTrailer { get; set; }

        public int? LeaseHaulerId { get; set; }
    }
}
