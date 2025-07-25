using Abp.Application.Services.Dto;

namespace DispatcherWeb.DynamicEntityProperties.Dto
{
    public class DynamicEntityPropertyValueDto : EntityDto<long>
    {
        public string Value { get; set; }

        public string EntityId { get; set; }

        public int DynamicEntityPropertyId { get; set; }
    }
}
