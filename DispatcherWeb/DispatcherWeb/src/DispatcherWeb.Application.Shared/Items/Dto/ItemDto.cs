namespace DispatcherWeb.Items.Dto
{
    public class ItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ItemType? Type { get; set; }

        public string TypeName => Type.GetDisplayName();

        public bool IsActive { get; set; }

        public bool DisallowDataMerge { get; set; }
    }
}
