using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public interface ILogoProvider
    {
        Task<byte[]> GetReportLogoAsBytesAsync(int? officeId);
    }
}
