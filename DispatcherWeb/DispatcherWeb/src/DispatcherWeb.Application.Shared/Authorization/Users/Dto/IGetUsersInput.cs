using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.Authorization.Users.Dto
{
    public interface IGetUsersInput : ISortedResultRequest
    {
        string Filter { get; set; }

        List<string> Permissions { get; set; }

        int? Role { get; set; }

        bool OnlyLockedUsers { get; set; }

        int? OfficeId { get; set; }
    }
}
