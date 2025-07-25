using System.Threading.Tasks;
using DispatcherWeb.Dto;
using DispatcherWeb.TempFiles.Dto;

namespace DispatcherWeb.TempFiles
{
    public interface ITempFileAppService
    {
        Task<TempFileDto> ProcessTempFile(ProcessTempFileInput input);
        Task DeleteTempFile(int tempFileId);
        Task<FileBytesDto> DownloadTempFile(int tempFileId);
    }
}
