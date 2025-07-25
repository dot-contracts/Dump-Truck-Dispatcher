namespace DispatcherWeb.DriverApp.Tickets.Dto
{
    public class TicketEditDto : TicketDto
    {
        public int Version { get; set; }
        public bool GenerateTicketNumber { get; set; }
    }
}
