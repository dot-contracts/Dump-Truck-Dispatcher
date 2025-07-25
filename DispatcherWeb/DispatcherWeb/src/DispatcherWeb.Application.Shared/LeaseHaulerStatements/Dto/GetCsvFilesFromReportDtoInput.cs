namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public class GetCsvFilesFromReportDtoInput
    {
        public bool SplitByLeaseHauler { get; set; }
        public LeaseHaulerStatementReportDto Report { get; set; }
    }
}
