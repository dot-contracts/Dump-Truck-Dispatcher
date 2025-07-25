using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.LeaseHaulers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulerUsers
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Users, AppPermissions.LeaseHaulerPortal)]
    public class LeaseHaulerUserAppService : DispatcherWebAppServiceBase, ILeaseHaulerUserAppService
    {
        private readonly IRepository<LeaseHaulerUser> _leaseHaulerUserRepository;
        private readonly IRepository<LeaseHauler> _leaseHaulerRepository;

        public LeaseHaulerUserAppService(
            IRepository<LeaseHaulerUser> leaseHaulerUserRepository,
            IRepository<LeaseHauler> leaseHaulerRepository)
        {
            _leaseHaulerUserRepository = leaseHaulerUserRepository;
            _leaseHaulerRepository = leaseHaulerRepository;
        }

        [AbpAuthorize(AppPermissions.LeaseHaulerPortal_MyCompany)]
        public async Task<LeaseHaulerDto> GetLeaseHaulerByUser()
        {
            var tenantId = await AbpSession.GetTenantIdAsync();
            var query = from leaseHaulerUser in (await _leaseHaulerUserRepository.GetQueryAsync())
                            .Where(q => q.UserId == Session.UserId && q.TenantId == tenantId && q.LeaseHaulerId == Session.LeaseHaulerId)
                        join leaseHauler in (await _leaseHaulerRepository.GetQueryAsync()).Where(q => q.TenantId == tenantId) on leaseHaulerUser.LeaseHaulerId equals leaseHauler.Id
                        select new LeaseHaulerDto
                        {
                            Id = leaseHauler.Id,
                            Name = leaseHauler.Name,
                            StreetAddress1 = leaseHauler.StreetAddress1,
                            StreetAddress2 = leaseHauler.StreetAddress2,
                            City = leaseHauler.City,
                            State = leaseHauler.State,
                            ZipCode = leaseHauler.ZipCode,
                            CountryCode = leaseHauler.CountryCode,
                            PhoneNumber = leaseHauler.PhoneNumber,
                            MailingAddress1 = leaseHauler.MailingAddress1,
                            MailingAddress2 = leaseHauler.MailingAddress2,
                            MailingCity = leaseHauler.MailingCity,
                            MailingState = leaseHauler.MailingState,
                            MailingZipCode = leaseHauler.MailingZipCode,
                            MailingCountryCode = leaseHauler.MailingCountryCode,
                            MotorCarrierNumber = leaseHauler.MotorCarrierNumber,
                            DeptOfTransportationNumber = leaseHauler.DeptOfTransportationNumber,
                            EinOrTin = leaseHauler.EinOrTin,
                            HireDate = leaseHauler.HireDate,
                            TerminationDate = leaseHauler.TerminationDate,
                        };
            return await query.FirstOrDefaultAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Users_Create, AppPermissions.Pages_Administration_Users_Edit, AppPermissions.LeaseHaulerPortal_MyCompany_Contacts)]
        public async Task UpdateLeaseHaulerUser(int? leaseHaulerId, long? userId, int? tenantId)
        {
            if (userId is null or 0)
            {
                throw new ArgumentException("User should be saved first");
            }

            if (!tenantId.HasValue)
            {
                throw new ArgumentException("Tenant ID is required");
            }

            var leaseHaulerUserLink = await _leaseHaulerUserRepository
                .FirstOrDefaultAsync(q =>
                    q.TenantId == tenantId.Value
                    && q.UserId == userId
                );

            if (leaseHaulerUserLink == null)
            {
                if (leaseHaulerId.HasValue)
                {
                    await CheckEntitySpecificPermissions(
                        AppPermissions.Pages_Administration_Users,
                        AppPermissions.LeaseHaulerPortal_MyCompany_Contacts,
                        Session.LeaseHaulerId,
                        leaseHaulerId.Value);

                    var leaseHaulerUser = new LeaseHaulerUser
                    {
                        TenantId = tenantId.Value,
                        LeaseHaulerId = leaseHaulerId.Value,
                        UserId = userId.Value,
                    };
                    await _leaseHaulerUserRepository.InsertAsync(leaseHaulerUser);
                }
            }
            else
            {
                await CheckEntitySpecificPermissions(
                    AppPermissions.Pages_Administration_Users,
                    AppPermissions.LeaseHaulerPortal_MyCompany_Contacts,
                    Session.LeaseHaulerId,
                    leaseHaulerUserLink.LeaseHaulerId,
                    leaseHaulerId);

                if (leaseHaulerId.HasValue)
                {
                    if (leaseHaulerUserLink.LeaseHaulerId == leaseHaulerId.Value)
                    {
                        return;
                    }

                    leaseHaulerUserLink.LeaseHaulerId = leaseHaulerId.Value;
                }
                else
                {
                    await _leaseHaulerUserRepository.HardDeleteAsync(leaseHaulerUserLink);
                }
            }
        }
    }
}
