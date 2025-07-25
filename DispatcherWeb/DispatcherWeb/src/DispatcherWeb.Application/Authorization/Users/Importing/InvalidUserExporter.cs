using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Dependency;
using DispatcherWeb.Authorization.Users.Importing.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;

namespace DispatcherWeb.Authorization.Users.Importing
{
    public class InvalidUserExporter : CsvExporterBase, IInvalidUserExporter, ITransientDependency
    {
        public InvalidUserExporter(ITempFileCacheManager tempFileCacheManager)
            : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(List<ImportUserDto> userListDtos)
        {
            return await CreateCsvFileAsync(
                "InvalidUserImportList.csv",
                () =>
                {
                    AddHeaderAndData(
                        userListDtos,
                        (L("UserName"), x => x.UserName),
                        (L("Name"), x => x.Name),
                        (L("Surname"), x => x.Surname),
                        (L("EmailAddress"), x => x.EmailAddress),
                        (L("PhoneNumber"), x => x.PhoneNumber),
                        (L("Password"), x => x.Password),
                        (L("Roles"), x => x.AssignedRoleNames?.JoinAsString(",")),
                        (L("Refuse Reason"), x => x.Exception)
                    );
                });
        }
    }
}
