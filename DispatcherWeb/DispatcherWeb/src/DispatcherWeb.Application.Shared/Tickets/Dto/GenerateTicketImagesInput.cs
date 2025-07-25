using System.Collections.Generic;

namespace DispatcherWeb.Tickets.Dto
{
    public class GenerateTicketImagesInput
    {
        public List<TicketPhotoDataDto> Tickets { get; set; }
        public string SuccessMessage { get; set; }
        public string FileName { get; set; }
    }
}
