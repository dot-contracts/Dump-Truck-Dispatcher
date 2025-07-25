namespace DispatcherWeb.Dto
{

    public class MustHaveTenantDto<TPrimaryKey>
    {
        public TPrimaryKey Id { get; set; }
        public int TenantId { get; set; }
    }

    public class MustHaveTenantDto : MustHaveTenantDto<int>
    {
    }
}
