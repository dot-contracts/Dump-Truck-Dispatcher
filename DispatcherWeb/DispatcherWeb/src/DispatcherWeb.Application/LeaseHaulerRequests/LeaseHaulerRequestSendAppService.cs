using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using DispatcherWeb.Authorization;
using DispatcherWeb.LeaseHaulerRequests.Dto;

namespace DispatcherWeb.LeaseHaulerRequests
{
    [AbpAuthorize(AppPermissions.Pages_LeaseHaulerRequests_Edit)]
    public class LeaseHaulerRequestSendAppService : DispatcherWebAppServiceBase, ILeaseHaulerRequestSendAppService
    {
        private readonly IRepository<LeaseHaulerRequest> _leaseHaulerRequestRepository;
        private readonly ILeaseHaulerNotifier _leaseHaulerNotifier;

        public LeaseHaulerRequestSendAppService(
            IRepository<LeaseHaulerRequest> leaseHaulerRequestRepository,
            ILeaseHaulerNotifier leaseHaulerNotifier
        )
        {
            _leaseHaulerRequestRepository = leaseHaulerRequestRepository;
            _leaseHaulerNotifier = leaseHaulerNotifier;
        }

        public async Task<bool> SendRequests(SendRequestsInput input)
        {
            bool success = true;
            var contacts = await _leaseHaulerNotifier.GetContacts(input.LeaseHaulerIds);
            if (!contacts.Any())
            {
                return false;
            }

            foreach (int leaseHaulerId in input.LeaseHaulerIds)
            {
                var currentLeaseHaulerContacts = contacts.Where(x => x.LeaseHaulerId == leaseHaulerId).ToList();
                if (currentLeaseHaulerContacts.Count == 0)
                {
                    await _leaseHaulerNotifier.NotifyLeaseHaulerHasNoDispatcherContactsError(leaseHaulerId);
                    success = false;
                    continue;
                }

                Guid guid = Guid.NewGuid();
                await CreateLeaseHaulerRequest(new SendLeaseHaulerRequestDto(input, leaseHaulerId, guid, input.Message));

                string message = $"{_leaseHaulerNotifier.GetAvailableTrucksUrl(guid)}\n{input.Message}";
                await _leaseHaulerNotifier.SendMessageToContacts(currentLeaseHaulerContacts, message);
            }

            return success;
        }

        private async Task CreateLeaseHaulerRequest(SendLeaseHaulerRequestDto model)
        {
            var leaseHaulerRequest = new LeaseHaulerRequest
            {
                Guid = model.Guid,
                Date = model.Date,
                Shift = model.Shift,
                OfficeId = model.OfficeId,
                LeaseHaulerId = model.LeaseHaulerId,
                Message = model.Message,
                Sent = model.Sent,
            };
            await _leaseHaulerRequestRepository.InsertAsync(leaseHaulerRequest);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
