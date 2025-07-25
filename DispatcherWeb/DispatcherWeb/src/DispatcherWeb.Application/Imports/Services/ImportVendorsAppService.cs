using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Locations;

namespace DispatcherWeb.Imports.Services
{
    public class ImportVendorsAppService : ImportDataBaseAppService<VendorImportRow>, IImportVendorsAppService
    {
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<LocationContact> _locationContactRepository;

        public ImportVendorsAppService(
            IRepository<Location> locationRepository,
            IRepository<LocationContact> locationContactRepository
        )
        {
            _locationRepository = locationRepository;
            _locationContactRepository = locationContactRepository;
        }

        protected override async Task<bool> ImportRowAsync(VendorImportRow row)
        {
            var existingLocation = (await _locationRepository.GetQueryAsync()).FirstOrDefault(x => x.Name == row.Name);
            if (existingLocation != null)
            {
                return false;
            }

            var location = new Location
            {
                IsActive = row.IsActive,
                Name = row.Name,
                StreetAddress = row.Address,
                City = row.City,
                State = row.State,
                ZipCode = row.ZipCode,
                CountryCode = row.CountryCode,
            };
            await _locationRepository.InsertAsync(location);

            if (!row.ContactName.IsNullOrEmpty() || !row.ContactPhone.IsNullOrEmpty())
            {
                var locationContact = new LocationContact
                {
                    Location = location,
                    Email = row.MainEmail,
                    Name = row.ContactName.IsNullOrEmpty() ? "Main contact" : row.ContactName,
                    Phone = row.ContactPhone,
                    Title = row.ContactTitle,
                };
                await _locationContactRepository.InsertAsync(locationContact);
            }

            if (!row.Contact2Name.IsNullOrEmpty() || !row.Contact2Phone.IsNullOrEmpty())
            {
                var locationContact = new LocationContact
                {
                    Location = location,
                    Name = row.Contact2Name.IsNullOrEmpty() ? "Alternative contact" : row.Contact2Name,
                    Phone = row.Contact2Phone,
                };
                await _locationContactRepository.InsertAsync(locationContact);
            }

            return true;
        }

        protected override bool IsRowEmpty(VendorImportRow row)
        {
            return row.Name.IsNullOrEmpty();
        }
    }
}
