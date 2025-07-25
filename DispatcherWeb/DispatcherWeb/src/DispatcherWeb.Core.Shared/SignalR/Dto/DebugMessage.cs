using Microsoft.Extensions.Logging;

namespace DispatcherWeb.SignalR.Dto
{
    public class DebugMessage
    {
        public DebugMessage()
        {
        }

        public DebugMessage(string message)
        {
            Message = message;
        }

        public DebugMessage(LogLevel logLevel, string message)
        {
            LogLevel = logLevel;
            Message = message;
        }

        public LogLevel LogLevel { get; set; } = LogLevel.Debug;
        public string Message { get; set; }
    }
}
