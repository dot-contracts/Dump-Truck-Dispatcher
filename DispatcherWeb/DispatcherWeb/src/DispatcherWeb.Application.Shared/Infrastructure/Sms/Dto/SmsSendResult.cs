using Abp.Domain.Entities;

namespace DispatcherWeb.Infrastructure.Sms.Dto
{
    public class SmsSendResult
    {
        public SmsSendResult()
        {
        }

        public SmsSendResult(
            string sid,
            SmsStatus status,
            int? errorCode,
            string errorMessage,
            Entity<int> sentSmsEntity = null
        )
        {
            Sid = sid;
            Status = status;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            SentSmsEntity = sentSmsEntity;
        }

        public string Sid { get; set; }
        public SmsStatus Status { get; set; }
        public int? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public Entity<int> SentSmsEntity { get; set; }
        public bool SentSmsEntityIsInserted { get; set; }
    }
}
