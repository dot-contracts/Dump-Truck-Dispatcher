namespace DispatcherWeb.QuickbooksDesktop.Dto
{
    public class ExportInvoicesToIIFResult
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
        public string ErrorMessage { get; set; }
    }
}
