namespace DispatcherWeb.MultiTenancy.Dto
{
    public class DeleteDispatchDataForTenantInput
    {
        public int Id { get; set; }
        public int? BatchSize { get; set; }
    }
}