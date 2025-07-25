using System;
using System.Threading.Tasks;
using DispatcherWeb.DriverApplication.Dto;

namespace DispatcherWeb.DriverApplication
{
    public interface IDriverApplicationAuthProvider
    {
        Task<AuthDriverByDriverGuidResult> AuthDriverByDriverGuid(Guid driverGuid);
        Task<AuthDriverByDriverGuidResult> AuthDriverByDriverGuidIfNeeded(Guid? driverGuid);
    }
}
