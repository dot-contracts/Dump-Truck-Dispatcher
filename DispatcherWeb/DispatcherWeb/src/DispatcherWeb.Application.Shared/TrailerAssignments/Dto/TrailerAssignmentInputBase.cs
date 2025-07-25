using System;

namespace DispatcherWeb.TrailerAssignments.Dto
{
    public class TrailerAssignmentInputBase
    {
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int? OfficeId { get; set; }

        public virtual void CopyValuesFrom(TrailerAssignmentInputBase other)
        {
            Date = other.Date;
            Shift = other.Shift;
            OfficeId = other.OfficeId;
        }
    }
}
