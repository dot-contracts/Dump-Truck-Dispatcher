using Abp.Application.Services.Dto;

namespace DispatcherWeb.Orders.Dto
{
    public class DeleteOrderLineInput : EntityDto
    {
        public DeleteOrderLineInput()
        {
        }

        public DeleteOrderLineInput(int id)
            : base(id)
        {
        }

        public int OrderId { get; set; }
    }
}
