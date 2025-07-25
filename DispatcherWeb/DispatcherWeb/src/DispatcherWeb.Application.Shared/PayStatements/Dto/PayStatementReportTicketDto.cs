using System;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementReportTicketDto : PayStatementReportItemDto
    {
        public DriverIsPaidForLoadBasedOnEnum DriverIsPaidForLoadBasedOn { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public DateTime? OrderDeliveryDate { get; set; }

        public override PayStatementItemKind ItemKind
        {
            get => PayStatementItemKind.Ticket;
            set { }
        }

        public override DateTime? Date
        {
            get
            {
                switch (DriverIsPaidForLoadBasedOn)
                {
                    case DriverIsPaidForLoadBasedOnEnum.TicketDate: return TicketDateTime;
                    case DriverIsPaidForLoadBasedOnEnum.OrderDate: return OrderDeliveryDate;
                    default: return null;
                }
            }

            set
            {
                switch (DriverIsPaidForLoadBasedOn)
                {
                    case DriverIsPaidForLoadBasedOnEnum.TicketDate:
                        TicketDateTime = value;
                        break;

                    case DriverIsPaidForLoadBasedOnEnum.OrderDate:
                        OrderDeliveryDate = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
