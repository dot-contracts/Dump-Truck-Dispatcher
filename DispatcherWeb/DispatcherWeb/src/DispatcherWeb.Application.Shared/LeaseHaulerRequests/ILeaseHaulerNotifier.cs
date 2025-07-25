using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.LeaseHaulerRequests.Dto;

namespace DispatcherWeb.LeaseHaulerRequests
{
    public interface ILeaseHaulerNotifier
    {
        Task NotifyLeaseHaulerDispatchers(NotifyLeaseHaulerInput input);
        Task<List<SendLeaseHaulerContactDto>> GetContacts(int[] leaseHaulerIds);
        string GetAvailableTrucksUrl(Guid guid);
        Task SendMessageToContacts(List<SendLeaseHaulerContactDto> contacts, string message);
        Task NotifyLeaseHaulerHasNoDispatcherContactsError(int leaseHaulerId);
    }
}
