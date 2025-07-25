using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.IO;
using DispatcherWeb.Logging.Dto;
using DispatcherWeb.Net.MimeTypes;

namespace DispatcherWeb.Logging
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Host_Maintenance)]
    public class WebLogAppService : DispatcherWebAppServiceBase, IWebLogAppService
    {
        private readonly IAppFolders _appFolders;
        private readonly ITempFileCacheManager _tempFileCacheManager;

        public WebLogAppService(IAppFolders appFolders, ITempFileCacheManager tempFileCacheManager)
        {
            _appFolders = appFolders;
            _tempFileCacheManager = tempFileCacheManager;
        }

        public async Task<GetLatestWebLogsOutput> GetLatestWebLogs()
        {
            var directory = new DirectoryInfo(_appFolders.WebLogsFolder);
            if (!directory.Exists)
            {
                return new GetLatestWebLogsOutput { LatestWebLogLines = new List<string>() };
            }

            var lastLogFile = directory
                .GetFiles("*.txt", SearchOption.AllDirectories)
                .Where(f => f.Name != "CspLogs.txt")
                .MaxBy(f => f.LastWriteTime);

            if (lastLogFile == null)
            {
                return new GetLatestWebLogsOutput();
            }

            var logLineCount = 0;

            var result = new List<string>();

            await foreach (var line in AppFileHelper.ReadLinesReverseAsync(lastLogFile.FullName))
            {
                if (line.StartsWith("DEBUG")
                    || line.StartsWith("INFO")
                    || line.StartsWith("WARN")
                    || line.StartsWith("ERROR")
                    || line.StartsWith("FATAL"))
                {
                    logLineCount++;
                }

                result.Add(line);

                if (logLineCount == 100)
                {
                    break;
                }
            }

            result.Reverse();

            return new GetLatestWebLogsOutput
            {
                LatestWebLogLines = result,
            };
        }

        public async Task<FileDto> DownloadWebLogs()
        {
            //Create temporary copy of logs
            var logFiles = GetAllLogFiles();

            using (var outputZipFileStream = new MemoryStream())
            {
                using (var zipStream = new ZipArchive(outputZipFileStream, ZipArchiveMode.Create))
                {
                    foreach (var logFile in logFiles)
                    {
                        var entry = zipStream.CreateEntry(logFile.Name);
                        await using (var entryStream = entry.Open())
                        {
                            await using (var fs = new FileStream(logFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
                            {
                                await fs.CopyToAsync(entryStream);
                                await entryStream.FlushAsync();
                            }
                        }
                    }
                }

                return await _tempFileCacheManager.StoreTempFileAsync(new FileBytesDto
                {
                    FileName = "WebSiteLogs.zip",
                    FileBytes = outputZipFileStream.ToArray(),
                    MimeType = MimeTypeNames.ApplicationZip,
                });
            }
        }

        private List<FileInfo> GetAllLogFiles()
        {
            var directory = new DirectoryInfo(_appFolders.WebLogsFolder);
            return directory.GetFiles("*.*", SearchOption.TopDirectoryOnly).ToList();
        }
    }
}
