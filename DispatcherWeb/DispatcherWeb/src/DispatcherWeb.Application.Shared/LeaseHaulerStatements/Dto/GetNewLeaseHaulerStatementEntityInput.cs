using System.Collections.Generic;

namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public class GetNewLeaseHaulerStatementEntityInput : AddLeaseHaulerStatementInput
    {
        public IEnumerable<INewLeaseHaulerStatementTicketDetailsDto> Tickets { get; set; }
    }
}
