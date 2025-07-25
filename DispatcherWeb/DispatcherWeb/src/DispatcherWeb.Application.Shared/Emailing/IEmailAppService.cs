using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Emailing.Dto;

namespace DispatcherWeb.Emailing
{
    public interface IEmailAppService : IApplicationService
    {
        Task TrackEvents(List<TrackEventInput> inputList);
        Task TrackEmailOpen(TrackEmailOpenInput input);
        Task<Guid> AddTrackableEmailAsync(MailMessage mail);
        Task<GetEmailHistoryInput> GetEmailHistoryInput(GetEmailHistoryInput input);
        Task<PagedResultDto<EmailHistoryDto>> GetEmailHistory(GetEmailHistoryInput input);
    }
}
