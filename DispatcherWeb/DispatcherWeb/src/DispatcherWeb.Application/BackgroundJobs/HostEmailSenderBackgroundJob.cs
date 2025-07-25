using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Net.Mail;
using Abp.Notifications;
using Abp.Timing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Emailing;
using DispatcherWeb.HostEmails;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Notifications;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.BackgroundJobs
{
    public class HostEmailSenderBackgroundJob : AsyncBackgroundJob<HostEmailSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IRepository<HostEmail> _hostEmailRepository;
        private readonly IRepository<HostEmailReceiver> _hostEmailReceiverRepository;
        private readonly TenantManager _tenantManager;
        private readonly UserManager _userManager;
        private readonly ITrackableEmailSender _trackableEmailSender;
        private readonly IAppNotifier _appNotifier;

        public HostEmailSenderBackgroundJob(
            IRepository<HostEmail> hostEmailRepository,
            IRepository<HostEmailReceiver> hostEmailReceiverRepository,
            TenantManager tenantManager,
            UserManager userManager,
            ITrackableEmailSender trackableEmailSender,
            IAppNotifier appNotifier
            )
        {
            _hostEmailRepository = hostEmailRepository;
            _hostEmailReceiverRepository = hostEmailReceiverRepository;
            _tenantManager = tenantManager;
            _userManager = userManager;
            _trackableEmailSender = trackableEmailSender;
            _appNotifier = appNotifier;
        }

        [UnitOfWork]
        public override async Task ExecuteAsync(HostEmailSenderBackgroundJobArgs args)
        {
            var hostEmail = await (await _hostEmailRepository.GetQueryAsync())
                .Include(x => x.Receivers)
                .Where(x => x.Id == args.HostEmailId)
                .FirstOrDefaultAsync();

            if (hostEmail == null)
            {
                Logger.Error($"HostEmail with Id {args.HostEmailId} wasn't found");
                await _appNotifier.SendMessageAsync(args.RequestorUser, "Sending host email failed, please try again", NotificationSeverity.Error);
                return;
            }

            var allowedTenantIds = await (await _tenantManager.GetQueryAsync())
                    .WhereIf(args.Input.ActiveFilter.HasValue, x => x.IsActive == args.Input.ActiveFilter)
                    .WhereIf(args.Input.EditionIds?.Any() == true, x => x.EditionId.HasValue && args.Input.EditionIds.Contains(x.EditionId.Value))
                    .Select(x => x.Id)
                .ToListAsync();

            if (args.Input.TenantIds.Any() == true)
            {
                allowedTenantIds = allowedTenantIds.Intersect(args.Input.TenantIds).ToList();
            }

            var receivers = new HashSet<HostEmailReceiver>();
            List<User> users;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                users = await (await _userManager.GetQueryAsync())
                    .Where(x => x.IsActive && x.TenantId.HasValue)
                    .Where(x => x.TenantId.HasValue && allowedTenantIds.Contains(x.TenantId.Value))
                .ToListAsync();

                foreach (var user in users.ToList())
                {
                    var userPreference = (HostEmailPreference)await SettingManager.GetSettingValueForUserAsync<int>(AppSettings.UserOptions.HostEmailPreference, user.ToUserIdentifier());
                    if (!userPreference.HasFlag((HostEmailPreference)(int)args.Input.Type))
                    {
                        users.Remove(user);
                        continue;
                    }
                }

                if (args.Input.RoleNames?.Any() == true)
                {
                    foreach (var user in users.ToList())
                    {
                        var hasAnyRole = false;
                        foreach (var roleName in args.Input.RoleNames)
                        {
                            if (await _userManager.IsInRoleAsync(user, roleName))
                            {
                                hasAnyRole = true;
                                break;
                            }
                        }
                        if (!hasAnyRole)
                        {
                            users.Remove(user);
                            continue;
                        }
                    }
                }
            }

            var from = await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress);

            foreach (var userGroup in users.GroupBy(x => x.TenantId.Value))
            {
                var tenantId = userGroup.Key;
                using (CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    foreach (var user in userGroup)
                    {
                        var receiver = new HostEmailReceiver
                        {
                            UserId = user.Id,
                            TenantId = tenantId,
                            HostEmailId = hostEmail.Id,
                        };
                        await _hostEmailReceiverRepository.InsertAsync(receiver);
                        try
                        {
                            receiver.TrackableEmailId = await _trackableEmailSender.SendTrackableAsync(new MailMessage(from, user.EmailAddress)
                            {
                                Subject = args.Input.Subject,
                                Body = args.Input.Body,
                                IsBodyHtml = false,
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.Message, ex);
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }

            hostEmail.ProcessedAtDateTime = Clock.Now;
            await CurrentUnitOfWork.SaveChangesAsync();

            await _appNotifier.SendMessageAsync(args.RequestorUser, $"Host Email finished processing (subject: {args.Input.Subject})", NotificationSeverity.Success);
        }
    }
}
