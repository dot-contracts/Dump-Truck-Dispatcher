namespace DispatcherWeb.Items.Dto
{
    public interface IGetItemListFilter
    {
        string Name { get; set; }
        FilterActiveStatus Status { get; set; }
        ItemType? Type { get; set; }
    }
}
