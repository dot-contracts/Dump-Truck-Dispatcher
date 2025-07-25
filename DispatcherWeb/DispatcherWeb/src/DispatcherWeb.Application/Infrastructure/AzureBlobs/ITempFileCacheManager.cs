using System.Threading.Tasks;
using Abp.Dependency;

namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public interface ITempFileCacheManager : ITransientDependency
    {
        Task<byte[]> GetFileAsync(string fileId);
        Task<string> SetFileAsync(byte[] content, string contentType = null);
    }
}
