using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Gdpr
{
    public interface IUserCollectedDataProvider
    {
        Task<List<FileDto>> GetFiles(UserIdentifier user);
    }
}
