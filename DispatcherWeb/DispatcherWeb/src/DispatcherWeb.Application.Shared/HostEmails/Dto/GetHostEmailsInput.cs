using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.HostEmails.Dto
{
    public class GetHostEmailsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public int? EditionId { get; set; }
        public int? TenantId { get; set; }
        public long? SentByUserId { get; set; }
        public HostEmailType? Type { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = nameof(HostEmailDto.SentAtDateTime);
            }
        }
    }
}
