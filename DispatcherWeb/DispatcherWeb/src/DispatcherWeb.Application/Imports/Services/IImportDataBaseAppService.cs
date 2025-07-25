using System.IO;
using System.Threading.Tasks;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;
using DispatcherWeb.Imports.Dto;

namespace DispatcherWeb.Imports.Services
{
    public interface IImportDataBaseAppService
    {
        IOfficeResolver OfficeResolver { get; set; }

        Task<ImportResultDto> Import(TextReader textReader,
            ImportJobArgs args);
    }
}
