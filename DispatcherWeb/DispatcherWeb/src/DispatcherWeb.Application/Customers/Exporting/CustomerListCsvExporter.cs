using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Customers.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;

namespace DispatcherWeb.Customers.Exporting
{
    public class CustomerListCsvExporter : CsvExporterBase, ICustomerListCsvExporter
    {
        public CustomerListCsvExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(List<CustomerDto> customerDtos)
        {
            return await CreateCsvFileAsync(
                "CustomerList.csv",
                () =>
                {
                    AddHeaderAndData(
                        customerDtos,
                        ("Name", x => x.Name),
                        ("Account", x => x.AccountNumber),
                        ("TIN", x => x.TaxIdNumber),
                        ("Address 1", x => x.Address1),
                        ("Address 2", x => x.Address2),
                        ("City", x => x.City),
                        ("State", x => x.State),
                        ("Zip Code", x => x.ZipCode),
                        ("Country Code", x => x.CountryCode),
                        ("Active", x => x.IsActive.ToYesNoString()),
                        ("Notes", x => x.Notes)
                    );
                }
            );
        }

    }
}
