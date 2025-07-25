using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.Configuration
{
    public interface IOfficeSettingsManager
    {
        Task<string> GetHeartlandPublicKeyAsync();
        Task<string> GetHeartlandSecretKeyAsync();
        Task<List<OfficeHeartlandKeys>> GetHeartlandKeysForOffices();
        Task<List<OfficeHeartlandKeys>> GetHeartlandKeysForOffices(IEnumerable<int> officeIds);
    }
}
