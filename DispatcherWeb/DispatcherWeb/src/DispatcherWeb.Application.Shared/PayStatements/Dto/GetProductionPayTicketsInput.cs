using System;

namespace DispatcherWeb.PayStatements.Dto
{
    public class GetProductionPayTicketsInput
    {
        public GetProductionPayTicketsInput()
        {
        }

        public GetProductionPayTicketsInput(AddPayStatementInput source)
        {
            StartDate = source.StartDate;
            EndDate = source.EndDate;
            OfficeId = source.OfficeId;
            LocalEmployeesOnly = source.LocalEmployeesOnly;
        }

        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? OfficeId { get; set; }
        public bool LocalEmployeesOnly { get; set; }
        public bool IncludeUnverified { get; set; }
        public bool ExcludeVerified { get; set; }
    }
}
