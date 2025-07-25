using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Notifications;
using DispatcherWeb.BackgroundJobs.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Tickets;
using DispatcherWeb.Tickets.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.BackgroundJobs
{
    public class TicketPhotoDownloadJob : DispatcherWebAsyncBackgroundJobBase<TicketPhotoDownloadJobArgs>, ITransientDependency
    {
        private readonly ITicketAppService _ticketAppService;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IRepository<Ticket> _ticketRepository;

        public TicketPhotoDownloadJob(
            ITicketAppService ticketAppService,
            IRepository<Ticket> ticketRepository,
            INotificationPublisher notificationPublisher,
            IExtendedAbpSession session
        ) : base(session)
        {
            _ticketAppService = ticketAppService;
            _ticketRepository = ticketRepository;
            _notificationPublisher = notificationPublisher;
        }

        public override async Task ExecuteAsync(TicketPhotoDownloadJobArgs args)
        {
            try
            {
                using (Session.Use(args.RequestorUser))
                {
                    await WithUnitOfWorkAsync(args.RequestorUser, async () =>
                    {
                        var ticketList = await (await _ticketRepository.GetQueryAsync())
                                        .Where(x => args.TicketIds.Contains(x.Id))
                                        .Select(x => new
                                        {
                                            TicketPhotoData = new TicketPhotoDataDto
                                            {
                                                TicketId = x.Id,
                                                TicketNumber = x.TicketNumber,
                                                TicketDateTime = x.TicketDateTime,
                                                TicketPhotoFilename = x.TicketPhotoFilename,
                                                TicketPhotoId = x.TicketPhotoId,
                                                IsInternal = x.IsInternal,
                                                LoadAtName = x.LoadAt.Name,
                                            },
                                        }).ToListAsync();
                        GenerateTicketImagesInput input = new GenerateTicketImagesInput
                        {
                            Tickets = ticketList.Select(x => x.TicketPhotoData).ToList(),
                            SuccessMessage = args.SuccessMessage,
                            FileName = args.FileName,
                        };
                        if (await SettingManager.GetSettingValueAsync<bool>(AppSettings.Tickets.PrintPdfTicket))
                        {
                            await _ticketAppService.GenerateTicketImagesPdf(input);
                        }
                        else
                        {
                            await _ticketAppService.GenerateTicketImagesZip(input);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(args.FailedMessage, ex);

                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.DownloadFileError,
                    new MessageNotificationData(args.FailedMessage),
                    null,
                    NotificationSeverity.Error,
                    userIds: new[]
                    {
                        args.RequestorUser,
                    }
                );
            }
        }
    }
}
