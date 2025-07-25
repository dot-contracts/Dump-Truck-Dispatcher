using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Authorization.Users.Exporting
{
    public interface IUserListCsvExporter
    {
        Task<FileDto> ExportToFileAsync(List<UserListDto> userListDtos);
    }
}
