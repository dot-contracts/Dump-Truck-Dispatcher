using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Infrastructure.AzureBlobs.Dto
{
    public class GetTempFileInput
    {
        [Required]
        public string FileName { get; set; }

        public string FileType { get; set; }

        [Required]
        public string FileToken { get; set; }
    }
}
