using System;
using System.Collections.Generic;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Dto
{
    public class GetDashboardDataInput : PagedAndSortedInputDto, IShouldNormalize//: DashboardInputBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ChartDateInterval IncomeStatisticsDateInterval { get; set; }
        public List<int?> EditionIds { get; set; }
        public FilterActiveStatus? Status { get; set; }
        public bool SuppressPaging { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrEmpty(Sorting))
            {
                Sorting = "TenantName";
                TrimTime();
            }


        }
        private void TrimTime()
        {
            StartDate = StartDate.Date;
            StartDate = StartDate.Date;

        }
    }
}
