using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class ZipExtensions
    {
        public static FileBytesDto ToZipFile(this IEnumerable<FileBytesDto> files, string zipFileName, CompressionLevel compressionLevel = CompressionLevel.NoCompression)
        {
            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Update, true))
            {
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(file.FileName, compressionLevel);
                    using (var entryStream = entry.Open())
                    {
                        entryStream.Write(file.FileBytes);
                    }
                }
            }

            return new FileBytesDto
            {
                FileBytes = zipStream.ToArray(),
                FileName = zipFileName,
                MimeType = "application/zip",
            };
        }
    }
}
