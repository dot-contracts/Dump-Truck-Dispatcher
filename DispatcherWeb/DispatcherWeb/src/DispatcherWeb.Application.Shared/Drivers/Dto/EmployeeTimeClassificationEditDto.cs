namespace DispatcherWeb.Drivers.Dto
{
    public class EmployeeTimeClassificationEditDto
    {
        public int Id { get; set; }
        public int TimeClassificationId { get; set; }
        public bool IsDefault { get; set; }
        public bool AllowForManualTime { get; set; }
        public decimal? PayRate { get; set; }
    }
}
