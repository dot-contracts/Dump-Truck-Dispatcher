using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Auditing.Dto
{
    public class GetEntityChangeInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string UserName { get; set; }

        public string EntityId { get; set; }

        public string EntityTypeFullName { get; set; }

        public void ValidateInput()
        {
            var valid = StartDate != null && EndDate != null
                        || !string.IsNullOrEmpty(EntityId) && !string.IsNullOrEmpty(EntityTypeFullName);

            if (!valid)
            {
                throw new ArgumentException("Either StartDate and EndDate or EntityId and EntityTypeFullName has to be specified");
            }
        }

        public void Normalize()
        {
            if (Sorting.IsNullOrWhiteSpace())
            {
                Sorting = "ChangeTime DESC";
            }
        }
    }
}
