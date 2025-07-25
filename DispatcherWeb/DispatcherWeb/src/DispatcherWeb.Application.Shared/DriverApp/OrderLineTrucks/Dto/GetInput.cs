using DispatcherWeb.Dto;

namespace DispatcherWeb.DriverApp.OrderLineTrucks.Dto
{
    public class GetInput : PagedInputDto
    {
        public int[] Ids { get; set; }
    }
}
