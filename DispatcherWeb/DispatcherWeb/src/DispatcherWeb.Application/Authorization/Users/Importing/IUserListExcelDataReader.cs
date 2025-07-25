using System.Collections.Generic;
using Abp.Dependency;
using DispatcherWeb.Authorization.Users.Importing.Dto;

namespace DispatcherWeb.Authorization.Users.Importing
{
    public interface IUserListExcelDataReader : ITransientDependency
    {
        List<ImportUserDto> GetUsersFromExcel(byte[] fileBytes);
    }
}
