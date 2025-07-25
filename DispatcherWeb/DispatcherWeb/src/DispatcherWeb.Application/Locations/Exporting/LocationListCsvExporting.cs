using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Locations.Dto;

namespace DispatcherWeb.Locations.Exporting
{
    public class LocationListCsvExporting : CsvExporterBase, ILocationListCsvExporting
    {
        public LocationListCsvExporting(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(List<LocationDto> locationDtos)
        {
            return await CreateCsvFileAsync(
                "LocationList.csv",
                () =>
                {
                    AddHeaderAndData(
                        locationDtos,
                        ("Name", x => x.Name),
                        ("Category", x => x.CategoryName),
                        ("Street Address", x => x.StreetAddress),
                        ("City", x => x.City),
                        ("State", x => x.State),
                        ("Zip Code", x => x.ZipCode),
                        ("Country Code", x => x.CountryCode),
                        ("Active", x => x.IsActive.ToYesNoString()),
                        ("Abbreviation", x => x.Abbreviation),
                        ("Notes", x => x.Notes)
                    );
                }
            );
        }

    }
}
