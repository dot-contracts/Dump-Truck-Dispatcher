using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.LeaseHaulers.Dto;

namespace DispatcherWeb.LeaseHaulers.Exporting
{
    public class LeaseHaulerListCsvExporter : CsvExporterBase, ILeaseHaulerListCsvExporter
    {
        public LeaseHaulerListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(List<LeaseHaulerEditDto> leaseHaulerEditDtos)
        {
            return await CreateCsvFileAsync(
                "LeaseHaulerList.csv",
                () =>
                {
                    AddHeaderAndData(
                        leaseHaulerEditDtos,
                        ("Name", x => x.Name),
                        ("Street Address 1", x => x.StreetAddress1),
                        ("Street Address 2", x => x.StreetAddress2),
                        ("City", x => x.City),
                        ("State", x => x.State),
                        ("Zip Code", x => x.ZipCode),
                        ("Country Code", x => x.CountryCode),
                        ("Account Number", x => x.AccountNumber),
                        ("Phone Number", x => x.PhoneNumber),
                        (L("IsActive"), x => x.IsActive.ToYesNoString())
                    );
                }
            );
        }

    }
}
