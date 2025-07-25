using System.Collections.Generic;

namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class UploadInvoicesResult
    {
        public int UploadedInvoicesCount { get; set; }
        public List<string> ErrorList { get; set; } = new List<string>();
    }
}
