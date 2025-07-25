using System;
using System.Threading.Tasks;
using Abp.Application.Services;

namespace DispatcherWeb.SecureFiles
{
    public interface ISecureFilesAppService : IApplicationService
    {
        Task<Guid> GetSecureFileDefinitionId();
    }
}
