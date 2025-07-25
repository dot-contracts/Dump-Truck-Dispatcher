using System;

namespace DispatcherWeb.Tickets.Dto
{
    public class GenerateTicketFilenameInput
    {
        public string TicketNumber { get; set; }

        public DateTime? TicketDateTime { get; set; }

        public string TicketPhotoFilename { get; set; }

        public Guid? TicketPhotoId { get; set; }

        public bool IsInternal { get; set; }

        public string LoadAtName { get; set; }
    }
}
