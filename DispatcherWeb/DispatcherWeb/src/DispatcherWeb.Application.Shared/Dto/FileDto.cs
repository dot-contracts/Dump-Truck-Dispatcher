using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Dto
{
    public class FileDto
    {
        [Required]
        public string FileName { get; set; }

        public string FileType { get; set; }

        public string FileToken { get; set; }

        public string WarningMessage { get; set; }

        public string SuccessMessage { get; set; }

        public FileDto()
        {

        }

        public FileDto(string fileName, string fileType)
        {
            FileName = fileName;
            FileType = fileType;
            FileToken = Guid.NewGuid().ToString("N");
        }
    }
}
