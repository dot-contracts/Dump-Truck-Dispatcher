using Abp.Application.Services.Dto;

namespace DispatcherWeb.DynamicEntityProperties.Dto
{
    public class DynamicPropertyValueDto : EntityDto<long>
    {
        public virtual string Value { get; set; }

        public int? TenantId { get; set; }

        public int DynamicPropertyId { get; set; }
    }
}
