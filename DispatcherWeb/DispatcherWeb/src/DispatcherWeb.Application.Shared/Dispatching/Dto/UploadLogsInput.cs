using System;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Dispatching.Dto
{
    public class UploadLogsInput
    {
        public long Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public string Level { get; set; } //obsolete
        public LogLevel? LogLevel { get; set; }
        public bool? Sw { get; set; }
        public string AppVersion { get; set; }
    }
}
