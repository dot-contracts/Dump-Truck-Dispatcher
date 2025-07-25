using System.Threading.Tasks;

namespace DispatcherWeb.Imports.DataResolvers.OfficeResolvers
{
    public interface IOfficeResolver
    {
        Task<int?> GetOfficeIdAsync(string officeStringValue);
    }
}
