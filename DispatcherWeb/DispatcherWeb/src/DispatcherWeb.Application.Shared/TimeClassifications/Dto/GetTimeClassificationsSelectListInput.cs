namespace DispatcherWeb.TimeClassifications.Dto
{
    public class GetTimeClassificationsSelectListInput
    {
        public bool ExcludeProductionPay { get; set; }

        public int? EmployeeId { get; set; }

        public bool? AllowForManualTime { get; set; }
    }
}
