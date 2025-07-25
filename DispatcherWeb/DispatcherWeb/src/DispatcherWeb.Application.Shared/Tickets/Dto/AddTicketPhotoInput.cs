using Abp.Auditing;

namespace DispatcherWeb.Tickets.Dto
{
    public class AddTicketPhotoInput
    {
        public int TicketId { get; set; }

        [DisableAuditing]
        public string TicketPhoto { get; set; }
        public string TicketPhotoFilename { get; set; }
    }
}
