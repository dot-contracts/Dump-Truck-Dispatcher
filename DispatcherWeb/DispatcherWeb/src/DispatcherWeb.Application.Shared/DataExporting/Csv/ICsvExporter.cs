using System.Threading.Tasks;
using DispatcherWeb.Dto;

namespace DispatcherWeb.DataExporting.Csv
{
    public interface ICsvExporter
    {
        Task<FileDto> StoreTempFileAsync(FileBytesDto fileBytes);
    }
}
