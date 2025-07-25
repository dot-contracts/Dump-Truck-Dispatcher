using System;
using System.Collections.Generic;

namespace DispatcherWeb.LeaseHaulerStatements.Dto
{
    public class AddLeaseHaulerStatementInput
    {
        public List<int> LeaseHaulerIds { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public T CopyTo<T>(T other) where T : AddLeaseHaulerStatementInput
        {
            other.LeaseHaulerIds = LeaseHaulerIds;
            other.StartDate = StartDate;
            other.EndDate = EndDate;
            return other;
        }
    }
}
