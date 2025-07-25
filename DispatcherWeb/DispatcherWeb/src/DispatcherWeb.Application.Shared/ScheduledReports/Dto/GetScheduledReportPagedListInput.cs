using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.ScheduledReports.Dto
{
    public class GetScheduledReportPagedListInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "ReportName";
            }

            // This is a hack to sort pdf, csv alphabetically
            if (Sorting.ToLowerInvariant() == "reportformat")
            {
                Sorting = "ReportFormat desc";
            }
            else if (Sorting.ToLowerInvariant() == "reportformat desc")
            {
                Sorting = "ReportFormat asc";
            }

        }

    }
}
