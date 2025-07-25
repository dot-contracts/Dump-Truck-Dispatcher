using System;
using Abp;
using Abp.Runtime.Session;

namespace DispatcherWeb.Runtime.Session
{
    public interface IExtendedAbpSession : IAbpSession
    {
        IDisposable Use(UserIdentifier userIdentifier);
        int? OfficeId { get; }
        string OfficeName { get; }
        bool OfficeCopyChargeTo { get; }
        int? CustomerId { get; }
        string CustomerName { get; }
        string UserName { get; }
        string UserEmail { get; }
        int? LeaseHaulerId { get; }
    }
}
