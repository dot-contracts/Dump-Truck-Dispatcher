using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Authorization.Users.Importing.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Authorization.Users.Importing
{
    public interface IInvalidUserExporter
    {
        Task<FileDto> ExportToFileAsync(List<ImportUserDto> userListDtos);
    }
}
