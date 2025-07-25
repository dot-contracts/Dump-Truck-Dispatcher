using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;

namespace DispatcherWeb.Authorization.Users.Exporting
{
    public class UserListCsvExporter : CsvExporterBase, IUserListCsvExporter
    {
        public UserListCsvExporter(
            ITempFileCacheManager tempFileCacheManager
        ) : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(List<UserListDto> userListDtos)
        {
            var timezone = await GetTimezone();
            return await CreateCsvFileAsync(
                "UserList.csv",
                () =>
                {
                    AddHeaderAndData(
                        userListDtos,
                        (L("Name"), x => x.Name),
                        (L("Surname"), x => x.Surname),
                        (L("UserName"), x => x.UserName),
                        (L("Office"), x => x.OfficeName),
                        (L("PhoneNumber"), x => x.PhoneNumber),
                        (L("EmailAddress"), x => x.EmailAddress),
                        (L("EmailConfirm"), x => x.IsEmailConfirmed.ToYesNoString()),
                        (L("Roles"), x => x.Roles.Select(r => r.RoleName).JoinAsString(", ")),
                        (L("LastLoginTime"), x => x.LastLoginTime?.ConvertTimeZoneTo(timezone).ToShortDateString()),
                        (L("Active"), x => x.IsActive.ToYesNoString()),
                        (L("CreationTime"), x => x.CreationTime.ConvertTimeZoneTo(timezone).ToShortDateString())
                    );

                }
            );
        }

    }
}
