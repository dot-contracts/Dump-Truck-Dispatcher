using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.DriverMessages.Dto
{
    public class GetDriverMessagePagedListInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public int? OfficeId { get; set; }
        public int? DriverId { get; set; }
        public long? UserId { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "TimeSent";
            }
        }

    }
}
