using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.Invoices.Dto
{
    public class GetCustomerTicketsResult : PagedResultDto<CustomerTicketDto>
    {
        public GetCustomerTicketsResult()
        {
        }

        public GetCustomerTicketsResult(int totalCount, IReadOnlyList<CustomerTicketDto> items) : base(totalCount, items)
        {
        }
    }
}
