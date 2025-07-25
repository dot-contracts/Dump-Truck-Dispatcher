namespace DispatcherWeb.LeaseHaulers.Dto.CrossTenantOrderSender
{
    public class ItemDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public ItemType? Type { get; set; }
    }
}
