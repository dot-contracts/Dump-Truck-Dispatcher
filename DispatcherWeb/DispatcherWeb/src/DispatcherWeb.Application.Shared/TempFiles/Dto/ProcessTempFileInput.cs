namespace DispatcherWeb.TempFiles.Dto
{
    public class ProcessTempFileInput
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string Message { get; set; }
    }
}
